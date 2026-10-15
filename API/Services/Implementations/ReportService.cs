using API.Domain.Abstractions;
using API.Domain.Entities;
using API.Domain.Enums;
using API.DTOs.Requests;
using API.DTOs.Responses;
using API.Errors.Exceptions;
using API.Infrastructure;
using API.Infrastructure.ModelBuilders;
using API.Services.Interfaces;
using API.Services.Validators;
using Contracts;
using Contracts.Messages;
using Contracts.Parameters;
using Microsoft.EntityFrameworkCore;
using Providers.OrgSphere;



namespace API.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;
        private readonly IUserContext _userContext;
        private readonly IOrgSphereProvider _orgSphereProvider;
        private readonly ISSRSService _ssrsService;
        private const string Role = "hse_dev";

        public ReportService(AppDbContext context, IOrgSphereProvider orgSphereProvider, IUserContext userContext, ISSRSService ssrsService)
        {
            _context = context;
            _userContext = userContext;
            _orgSphereProvider = orgSphereProvider;
            _ssrsService = ssrsService;
        }

        private async Task CheckAccessAsync()
        {
            var roleCheck = await _orgSphereProvider.CheckUserRoleAsync(Role);

            if (roleCheck.IsFailed)
                throw new UnauthorizedAccessException("Доступ запрещён.");
        }

        public async Task<SearchResult<ReportDto>> GetAllAsync(IPagination pagination)
        {
            var reports = _context.Reports
                        .Where(r => r.StatusCode == RecordStatus.Active)
                        .Include(r => r.Parameters.Where(p => p.StatusCode == RecordStatus.Active))
                        .AsNoTracking();

            var totalCount = await reports.CountAsync();

            var data = await reports
                        .ApplyPagination(pagination)
                        .Select(r => ResponseMapper.ToResponseDto(r))
                        .ToListAsync();

            return new SearchResult<ReportDto>
            {
                Data = data,
                TotalCount = totalCount
            };
        }

        public async Task<ReportDto?> GetByIdAsync(int id)
        {
            var report = await _context.Reports
                         .Include(r => r.Parameters.Where(p => p.StatusCode == RecordStatus.Active))
                         .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
                throw new NotFoundException("Отчет не найден.");

            return ResponseMapper.ToResponseDto(report);
        }

        public async Task<ReportDto> CreateAsync(CreateReportDto dto)
        {
            await CheckAccessAsync();

            ReportParameterValidator.Validate(dto);

            var exists = await _context.Reports
                                 .AnyAsync(r =>
                                 r.StatusCode == RecordStatus.Active &&
                                 r.Name == dto.Name &&
                                 r.SsrsReportPath == dto.SsrsReportPath );

            if (exists)
                throw new ArgumentException("Отчет с таким именем и SSRS путем уже существует.");


            await SsrsReportValidator.ReportExistsAsync(_ssrsService, dto.SsrsReportPath);

            var ssrsParams = await _ssrsService.GetReportParameter(dto.SsrsReportPath);

            var newReport = new Report();
            newReport.StatusCode = RecordStatus.Active;
            newReport.CreatedOn = DateTime.UtcNow;
            newReport.CreatedBy = (Guid)_userContext.UserId;
            newReport.Name = dto.Name;
            newReport.Description = dto.Description;
            newReport.SsrsReportPath = dto.SsrsReportPath;
            newReport.Parameters = ssrsParams.Select(p => new ReportParameter
            {
                Name = p.Name,
                DisplayName = p.Name,
                Type = Enum.Parse<ParameterType>(p.ParameterType, ignoreCase: true),
                IsRequired = false,
                DefaultValue = null,
                CreatedBy = _userContext.UserId.Value,
            }).ToList();

            _context.Reports.Add(newReport);    

            await _context.SaveChangesAsync();
            return ResponseMapper.ToResponseDto(newReport);
        }

        public async Task<ReportDto> UpdateAsync(int id, UpdateReportDto input)
        {
            await CheckAccessAsync();
            ReportParameterValidator.Validate(input);

            var updatedReport = await _context.Reports
                .Include(r => r.Parameters)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (updatedReport == null)
                throw new NotFoundException("Отчет не найден.");

            var exists = await _context.Reports
                                .AnyAsync(r =>
                                r.Id != id &&
                                r.Name == input.Name);

            if (exists)
                throw new ArgumentException("Отчет с таким именем и SSRS путем уже существует.");

            updatedReport.Name = input.Name;
            updatedReport.Description = input.Description;

            await _context.SaveChangesAsync();
            return ResponseMapper.ToResponseDto(updatedReport);
        }

        public async Task<ReportDto> UpdateParametersAsync(int id)
        {
            await CheckAccessAsync();

            var report = await _context.Reports
                .Include(r => r.Parameters)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
                throw new NotFoundException("Отчет не найден.");

            var ssrsParams = await _ssrsService.GetReportParameter(report.SsrsReportPath);
            var ssrsNames = ssrsParams
                .Select(p => p.Name.ToLowerInvariant())
                .ToHashSet();

            var dateNow = DateTime.UtcNow;

            foreach (var p in report.Parameters.Where(p => p.StatusCode == RecordStatus.Active))
            {
                if (!ssrsNames.Contains(p.Name.ToLowerInvariant()))
                {
                    p.StatusCode = RecordStatus.Inactive;
                    p.DeletedOn = dateNow;
                    p.DeletedBy = _userContext.UserId.Value;
                    p.UpdatedBy = _userContext.UserId.Value;
                }
            }

            var existingNames = report.Parameters
                .Select(p => p.Name.ToLowerInvariant())
                .ToHashSet();

            foreach (var sp in ssrsParams)
            {
                if (!existingNames.Contains(sp.Name.ToLowerInvariant()))
                {
                    report.Parameters.Add(new ReportParameter
                    {
                        Name = sp.Name,
                        DisplayName = sp.Name,
                        Type = Enum.Parse<ParameterType>(sp.ParameterType, ignoreCase: true),
                        IsRequired = false,
                        DefaultValue = null,
                        StatusCode = RecordStatus.Active,
                        CreatedBy = _userContext.UserId.Value,
                    });
                }
            }

            await _context.SaveChangesAsync();
            return ResponseMapper.ToResponseDto(report);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await CheckAccessAsync();

            var report = await _context.Reports
                .Include(r => r.Parameters)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
                throw new NotFoundException("Отчет не найден.");

            var dateNow = DateTime.UtcNow;
            report.StatusCode = RecordStatus.Inactive;
            report.UpdatedOn = dateNow;
            report.DeletedOn = dateNow;
            report.DeletedBy = (Guid)_userContext.UserId;

            foreach (var p in report.Parameters)
            {
                p.StatusCode = RecordStatus.Inactive;
                p.DeletedOn = dateNow;
                p.DeletedBy = _userContext.UserId.Value;
                p.UpdatedBy = _userContext.UserId.Value;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}