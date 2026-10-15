namespace API.DTOs.Responses;

public record ReportFileResult
{
    public required byte[] FileContent { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
}