using API.Domain.Entities;
using API.Domain.Enums;
using API.DTOs.Requests;
using API.DTOs.Responses;
using API.Errors.Exceptions;
using API.Infrastructure.ModelBuilders;
using API.Services.Interfaces;
using Contracts;
using System.Linq;
using Contracts.Parameters;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Providers.OrgSphere;
using Xunit;


namespace ReportService.Tests;

public class ReportServiceTest : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IOrgSphereProvider> _orgSphereMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<ISSRSService> _ssrsServiceMock;
    private readonly Mock<IPagination> _paginationMock;

    private readonly API.Services.Implementations.ReportService _sut;

    private static readonly Guid _userId = Guid.Empty;

    public ReportServiceTest()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        _orgSphereMock = new Mock<IOrgSphereProvider>();
        _userContextMock = new Mock<IUserContext>();
        _ssrsServiceMock = new Mock<ISSRSService>();
        _paginationMock = new Mock<IPagination>();
        _paginationMock.Setup(x => x.Skip).Returns(0);
        _paginationMock.Setup(x => x.Take).Returns(25);

        _userContextMock.Setup(x => x.UserId).Returns(_userId);

        _orgSphereMock
            .Setup(x => x.CheckUserRoleAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Ok());

        _ssrsServiceMock
            .Setup(x => x.GetSsrsList())
            .ReturnsAsync(new List<SsrsReportDto>());

        _ssrsServiceMock
            .Setup(x => x.GetReportParameter(It.IsAny<string>()))
            .ReturnsAsync(new List<SsrsParameterDefinition>());

        _sut = new API.Services.Implementations.ReportService(
            _context,
            _orgSphereMock.Object,
            _userContextMock.Object,
            _ssrsServiceMock.Object);
    }

    public void Dispose() => _context.Dispose();

    //Get
    [Fact]
    public async Task GetAllAsync_ReturnsOnlyActiveReports()
    {
        _context.Reports.AddRange(
            MakeReport("Active1", RecordStatus.Active),
            MakeReport("Inactive", RecordStatus.Inactive),
            MakeReport("Active2", RecordStatus.Active));
        await _context.SaveChangesAsync();

        var result = await _sut.GetAllAsync(_paginationMock.Object);

        Assert.Equal(2, result.Data.Count());
        Assert.All(result.Data, r => Assert.NotEqual("Inactive", r.Name));
    }


    [Fact]
    public async Task GetAllAsync_ReturnEmptyList()
    {
        var result = await _sut.GetAllAsync(_paginationMock.Object);
        Assert.Empty(result.Data);
    }


    [Fact]
    public async Task GetAllAsync_IncludesParameters()
    {
        var report = MakeReport("R1", RecordStatus.Active);
        report.Parameters.Add(new ReportParameter
        {
            Name = "param1",
            DisplayName = "Param 1",
            Type = ParameterType.String,
            IsRequired = true,
            CreatedBy = _userId,
            UpdatedBy = _userId
        });
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        var result = await _sut.GetAllAsync(_paginationMock.Object);

        Assert.Single(result.Data);
        var data = result.Data.ToList();
        Assert.Single(data[0].Parameters);
        Assert.Equal("param1", data[0].Parameters[0].Name);
    }

    //GetById
    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsReport()
    {
        var report = MakeReport("Test");
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(report.Id);

        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
    }


    [Fact]
    public async Task GetByIdAsync_NonExistingId_Exception()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(999));
    }

    //POST
    [Fact]
    public async Task CreateAsync_CreatesAndReturnsReport()
    {
        SetupSsrsWithReport("/HSE/MyReport");

        var dto = new CreateReportDto
        {
            Name = "New Report",
            Description = "Desc",
            SsrsReportPath = "/HSE/MyReport",
            Parameters = new()
        };

        var result = await _sut.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("New Report", result.Name);
        Assert.Equal(1, _context.Reports.Count());
    }


    [Fact]
    public async Task CreateAsync_SavesParameters()
    {
        SetupSsrsWithReport("/HSE/Rep", ("StartDate", "DateTime"));

        var dto = new CreateReportDto
        {
            Name = "Report",
            SsrsReportPath = "/HSE/Rep",
            Parameters = new()
            {
                new CreateReportParameterDto
                {
                    Name        = "StartDate",
                    DisplayName = "Start Date",
                    Type        = ParameterType.DateTime,
                    IsRequired  = true
                }
            }
        };

        var result = await _sut.CreateAsync(dto);

        Assert.Single(result.Parameters);
        Assert.Equal("StartDate", result.Parameters[0].Name);
        Assert.Equal(ParameterType.DateTime, result.Parameters[0].Type);
    }


    [Fact]
    public async Task CreateAsync_Duplicates_Exception()
    {
        _context.Reports.Add(MakeReport("Existing", RecordStatus.Active, "/HSE/Path"));
        await _context.SaveChangesAsync();

        SetupSsrsWithReport("/HSE/Path");

        var dto = new CreateReportDto
        {
            Name = "Existing",
            SsrsReportPath = "/HSE/Path"
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(dto));
    }


    [Fact]
    public async Task CreateAsync_UnauthorizedException()
    {
        _orgSphereMock
            .Setup(x => x.CheckUserRoleAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Fail("Forbidden"));

        var dto = new CreateReportDto
        {
            Name = "Report",
            SsrsReportPath = "/HSE/Path"
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.CreateAsync(dto));
    }


    [Fact]
    public async Task CreateAsync_InvalidSsrsPathException()
    {
        _ssrsServiceMock
            .Setup(x => x.GetSsrsList())
            .ReturnsAsync(new List<SsrsReportDto>());

        var dto = new CreateReportDto
        {
            Name = "Report",
            SsrsReportPath = "/HSE/GhostPath"
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(dto));
    }


    [Fact]
    public async Task CreateAsync_InvalidParameterException()
    {
        SetupSsrsWithReport("/HSE/Rep", ("Amount", "Int"));

        var dto = new CreateReportDto
        {
            Name = "Report",
            SsrsReportPath = "/HSE/Rep",
            Parameters = new()
            {
                new CreateReportParameterDto
                {
                    Name         = "Amount",
                    DisplayName  = "Amount",
                    Type         = ParameterType.Int,
                    DefaultValue = "not-a-number"
                }
            }
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(dto));
    }

    //PUT
    [Fact]
    public async Task UpdateAsync_ExistingReportUpdates()
    {
        var report = MakeReport("Old Name", ssrsPath: "/HSE/Old");
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        SetupSsrsWithReport("/HSE/New");

        var dto = new UpdateReportDto
        {
            Name = "New Name",
            Description = "New Desc",
            SsrsReportPath = "/HSE/New"
        };

        var result = await _sut.UpdateAsync(report.Id, dto);

        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
        Assert.Equal("New Desc", result.Description);
        Assert.Equal("/HSE/New", result.SsrsReportPath);
    }


    [Fact]
    public async Task UpdateAsync_NonExistingId_Exception()
    {
        var dto = new UpdateReportDto
        {
            Name = "X",
            SsrsReportPath = "/HSE/X"
        };

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateAsync(9999, dto));
    }


    [Fact]
    public async Task UpdateAsync_Duplicates_Exception()
    {
        _context.Reports.AddRange(
            MakeReport("Report A", ssrsPath: "/HSE/A"),
            MakeReport("Report B", ssrsPath: "/HSE/B"));
        await _context.SaveChangesAsync();

        var reportB = _context.Reports.First(r => r.Name == "Report B");

        var dto = new UpdateReportDto
        {
            Name = "Report A",
            SsrsReportPath = "/HSE/A"
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateAsync(reportB.Id, dto));
    }


    [Fact]
    public async Task UpdateAsync_AddNewParameter()
    {
        var report = MakeReport("R", ssrsPath: "/HSE/R");
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        SetupSsrsWithReport("/HSE/R", ("Flag", "Boolean"));

        var dto = new UpdateReportDto
        {
            Name = "R",
            SsrsReportPath = "/HSE/R",
            Parameters = new()
            {
                new UpdateReportParameterDto
                {
                    Name        = "Flag",
                    DisplayName = "Flag",
                    Type        = ParameterType.Boolean
                }
            }
        };

        var result = await _sut.UpdateAsync(report.Id, dto);

        Assert.Single(result.Parameters);
        Assert.Equal("Flag", result.Parameters[0].Name);
    }


    //DELETE
    [Fact]
    public async Task DeleteAsync_SetsInactive()
    {
        var report = MakeReport("ToDelete");
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        var deleted = await _sut.DeleteAsync(report.Id);

        Assert.True(deleted);
        var fromDb = await _context.Reports.FindAsync(report.Id);
        Assert.Equal(RecordStatus.Inactive, fromDb!.StatusCode);
        Assert.NotNull(fromDb.DeletedOn);
        Assert.Equal(_userId, fromDb.DeletedBy);
    }


    [Fact]
    public async Task DeleteAsync_NonExistingIdException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(9999));
    }


    [Fact]
    public async Task DeleteAsync_SetsParametersInactive()
    {
        var report = MakeReport("WithParams");
        report.Parameters.Add(new ReportParameter
        {
            Name = "P1",
            DisplayName = "P1",
            Type = ParameterType.String,
            CreatedBy = _userId,
            UpdatedBy = _userId
        });
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        await _sut.DeleteAsync(report.Id);

        var param = _context.ReportParameters.First();
        Assert.Equal(RecordStatus.Inactive, param.StatusCode);
        Assert.NotNull(param.DeletedOn);
    }



    private static Report MakeReport(
        string name,
        RecordStatus status = RecordStatus.Active,
        string ssrsPath = "/HSE/Default")
    {
        return new Report
        {
            Name = name,
            Description = "desc",
            SsrsReportPath = ssrsPath,
            StatusCode = status,
            CreatedBy = _userId,
            UpdatedBy = _userId
        };
    }

    private void SetupSsrsWithReport(string reportPath, params (string Name, string Type)[] parameters)
    {
        _ssrsServiceMock
            .Setup(x => x.GetSsrsList())
            .ReturnsAsync(new List<SsrsReportDto>
            {
                new() { Name = "Report", ReportPath = reportPath, Type = "Report" }
            });

        _ssrsServiceMock
            .Setup(x => x.GetReportParameter(reportPath))
            .ReturnsAsync(parameters
                .Select(p => new SsrsParameterDefinition { Name = p.Name, ParameterType = p.Type })
                .ToList());
    }
}