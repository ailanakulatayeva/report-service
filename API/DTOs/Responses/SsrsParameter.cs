namespace API.DTOs.Responses
{
    public class SsrsParameter
    {
        public List<SsrsParameterDefinition> Value { get; set; } = [];
    }

    public class SsrsParameterDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string ParameterType { get; set; } = string.Empty;
    }

}
