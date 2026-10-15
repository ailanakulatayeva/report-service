using API.DTOs.Responses;

namespace API.Services.Interfaces;

public interface ISSRSService
{
    Task<ReportFileResult> RenderReportAsync(
        int reportId,
        Dictionary<string, string> parameterValues,
        CancellationToken cancellationToken);

    Task<List<SsrsReportDto>> GetSsrsList();
    Task<List<SsrsParameterDefinition>> GetReportParameter(string reportPath);
}