using API.Domain.Entities;
using API.Domain.Enums;

namespace API.DTOs.Responses
{
    public class ReportParameterDto (ReportParameter parameter)
    {
        public int Id { get; set; } = parameter.Id;
        public string Name { get; set; } = parameter.Name;

        public string DisplayName { get; set; } = parameter.DisplayName;

        public ParameterType Type { get; set; } = parameter.Type;

        public bool IsRequired { get; set; } = parameter.IsRequired;

        public string? DefaultValue { get; set; } = parameter.DefaultValue;
      
    }
}
