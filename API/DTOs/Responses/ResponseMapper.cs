using API.Domain.Entities;

namespace API.DTOs.Responses;

public static class ResponseMapper
{
    public static ReportDto ToResponseDto(Report r)
    {
        var dto = new ReportDto(r);

        dto.Parameters = r.Parameters?
            .Select(p => new ReportParameterDto(p))
            .ToList();

        return dto;
    }
}