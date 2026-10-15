using API.DTOs.Requests;

namespace API.Services.Validators;

public static class ReportParameterValidator
{
    public static void Validate(CreateReportDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Название отчёта является обязательным полем.");

        if (string.IsNullOrWhiteSpace(dto.SsrsReportPath))
            throw new ArgumentException("SSRS путь является обязательным полем.");
    }

    public static void Validate(UpdateReportDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Название отчёта является обязательным полем.");

        if (string.IsNullOrWhiteSpace(dto.Description))
            throw new ArgumentException("Описание отчёта является обязательным полем.");
    }
}