using API.Domain.Entities;
using API.Domain.Enums;

namespace API.DTOs.Responses;

public class ReportDto(Report report)
{
    public int Id { get; set; } = report.Id;
    public string Name { get; set; } = report.Name;
    public string? Description { get; set; } = report.Description;
    public string SsrsReportPath { get; set; } = report.SsrsReportPath;
    public DateTime CreatedOn { get; set; } = report.CreatedOn;
    public DateTime UpdatedOn { get; set; } = report.UpdatedOn;
    public DateTime? DeletedOn { get; set; } = report.DeletedOn;
    public Guid CreatedBy { get; set; } = report.CreatedBy;
    public Guid UpdatedBy { get; set; } = report.UpdatedBy;
    public Guid? DeletedBy { get; set; } = report.DeletedBy;
    public RecordStatus StatusCode { get; set; } = report.StatusCode;

    public List<ReportParameterDto> Parameters { get; set; }

}
