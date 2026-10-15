namespace API.DTOs.Requests;

public record CreateReportDto
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public required string SsrsReportPath { get; set; }
}
