using API.Configurations;
using API.DTOs.Responses;
using API.Errors.Exceptions;
using API.Infrastructure.ModelBuilders;
using API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.SqlServer.Server;
using System.Buffers.Text;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace API.Services.Implementations;

public class SSRSService(
    AppDbContext context,
    IHttpClientFactory httpClientFactory,
    IOptions<SSRSConfigurations> options,
    ILogger<SSRSService> logger) : ISSRSService
{
    public static readonly string ClientName = "SSRSClient";
    private readonly HttpClient _client = httpClientFactory.CreateClient(ClientName);

    public async Task<ReportFileResult> RenderReportAsync(
        int reportId,
        Dictionary<string, string> parameterValues,
        CancellationToken cancellationToken)
    {
        var report = await context.Reports
            .Include(r => r.Parameters)
            .FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken)
            ?? throw new NotFoundException($"Отчет с id {reportId} не найден.");

        var reportUrl = $"{options.Value.BaseUrl}/ReportServerMES?{report.SsrsReportPath}";
        //&permit_id={ permitId}";
        ///HSE/WorkPermit/WorkPermitDev

        var reqParams = BuildParameterQuery(report.Parameters, parameterValues);

        using var response = await _client.GetAsync($"{reportUrl}{reqParams}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError($"Ошибка получения отчета из SSRS. ReportUrl = [{reportUrl}]. Response = [{JsonSerializer.Serialize(response)}].");
            throw new ProviderException("Ошибка получения отчета из SSRS.");
        }

        var fileContent = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        return new ReportFileResult
        {
            FileContent = fileContent,
            FileName = $"{report.Name}.pdf",
            ContentType = "application/pdf"
        };
    }

    private static string BuildParameterQuery(
        IEnumerable<Domain.Entities.ReportParameter> dbParameters,
        Dictionary<string, string> passedValues)
    {
        var parts = new List<string> { "&rs:Command=Render&rs:Format=PDF" };

        foreach (var param in dbParameters)
        {
            if (passedValues.TryGetValue(param.Name, out var value) && !string.IsNullOrWhiteSpace(value))
                parts.Add($"{Uri.EscapeDataString(param.Name)}={Uri.EscapeDataString(value)}");
            else if (!string.IsNullOrWhiteSpace(param.DefaultValue))
                parts.Add($"{Uri.EscapeDataString(param.Name)}={Uri.EscapeDataString(param.DefaultValue)}");
        }

        return string.Join("&", parts);
    }

    public async Task<List<SsrsReportDto>> GetSsrsList()
    {
        var url = $"{options.Value.BaseUrl}/ReportsMES/api/v2.0/Reports";

        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var apiResponse = JsonSerializer.Deserialize<SsrsCatalogResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var result = apiResponse?.Value?
            .Where(item => item.Path.StartsWith("/HSE", StringComparison.OrdinalIgnoreCase))
            .Select(item => new SsrsReportDto
            {
                Name = item.Name,
                ReportPath = $"{item.Path}",
                Type = item.Type
            })
            .ToList() ?? [];

        return result;
    }
        
    public class SsrsCatalogResponse
    {
        public List<SsrsCatalogItem> Value { get; set; } = [];
    }

    public class SsrsCatalogItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public async Task<List<SsrsParameterDefinition>> GetReportParameter(string reportPath)
    {
        var listUrl = $"{options.Value.BaseUrl}/ReportsMES/api/v2.0/Reports";
        var listResponse = await _client.GetAsync(listUrl);
        listResponse.EnsureSuccessStatusCode();

        var listContent = await listResponse.Content.ReadAsStringAsync();
        var catalog = JsonSerializer.Deserialize<SsrsCatalogResponse>(listContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var report = catalog?.Value?
            .FirstOrDefault(r => r.Path.Equals(reportPath, StringComparison.OrdinalIgnoreCase));


        var paramUrl = $"{options.Value.BaseUrl}/ReportsMES/api/v2.0/Reports({report.Id})/ParameterDefinitions";
        var paramResponse = await _client.GetAsync(paramUrl);
        if (!paramResponse.IsSuccessStatusCode)
            throw new Exception($"Ошибка {(int)paramResponse.StatusCode}: {await paramResponse.Content.ReadAsStringAsync()}");

        var paramContent = await paramResponse.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<SsrsParameter>(paramContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result?.Value ?? [];
    }

}



