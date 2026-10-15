using API.DTOs.Requests;
using API.DTOs.Responses;
using Contracts.Messages;
using Contracts.Parameters;

namespace API.Services.Interfaces
{
    public interface IReportService
    {
        Task<SearchResult<ReportDto>> GetAllAsync(IPagination pagination);
        Task<ReportDto?> GetByIdAsync(int id);
        Task<ReportDto> CreateAsync(CreateReportDto dto);
        Task<ReportDto?> UpdateAsync(int id, UpdateReportDto input);
        Task<ReportDto> UpdateParametersAsync(int id);
        Task<bool> DeleteAsync(int id);

    }
}
