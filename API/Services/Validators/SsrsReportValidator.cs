using API.Services.Interfaces;

namespace API.Services.Validators
{
    public static class SsrsReportValidator
    {
        public static async Task ReportExistsAsync(ISSRSService ssrsService, string ssrsReportPath)
        { 
            if (string.IsNullOrWhiteSpace(ssrsReportPath))
                throw new ArgumentException("SSRS путь пустой.");

            var list = await ssrsService.GetSsrsList();
            var exists = list.Any(x =>
                x.ReportPath.Equals(ssrsReportPath, StringComparison.OrdinalIgnoreCase));
            if (!exists)
                throw new ArgumentException("Отчет не найден на SSRS сервере.");
        }
    }
}