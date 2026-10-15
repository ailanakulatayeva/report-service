namespace API.DTOs.Requests;

public class UpdateReportDto
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;

}
