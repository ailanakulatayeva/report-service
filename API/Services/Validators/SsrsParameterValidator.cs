using API.Domain.Enums;
using API.Services.Interfaces;
namespace API.Services.Validators;

public static class SsrsParameterValidator
{
    public static async Task ValidateAsync(
        ISSRSService ssrsService,
        string ssrsReportPath,
        IEnumerable<(string Name, ParameterType Type)> parameters)
    {
        var ssrsParams = await ssrsService.GetReportParameter(ssrsReportPath);

        foreach (var (name, type) in parameters)
        { 
            var ssrsParam = ssrsParams
                .FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (ssrsParam == null)
            {
                var availableParam = string.Join(", ", ssrsParams.Select(p => p.Name));
                throw new ArgumentException(
                    $"Параметр '{name}' не найден в SSRS. Доступно {availableParam}.");
            }

            var ssrsType = ssrsParam.ParameterType.ToLowerInvariant();
            var ourType = type.ToString().ToLowerInvariant();

            if (ssrsType != ourType)
                throw new ArgumentException(
                    $"Параметр '{name}' в SSRS имеет тип '{ssrsParam.ParameterType}', вы передали '{type}'.");
        }
    }
}