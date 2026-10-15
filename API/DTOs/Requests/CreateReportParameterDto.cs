using API.Domain.Entities;
using API.Domain.Enums;

namespace API.DTOs.Requests;

public class CreateReportParameterDto
{
    public required string Name { get; set;}

    public required string DisplayName { get; set; } 

    public ParameterType Type { get; set; }

    public bool IsRequired { get; set; }

    public string DefaultValue { get; set; } =string.Empty;

}
