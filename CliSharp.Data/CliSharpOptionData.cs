namespace CliSharp.Data
{
    public class CliSharpOptionData
    {
        public string? Id { get; set; }
        public string? Description { get; set; }
        public string? Shortcut { get; set; }
        public List<CliSharpParameterData>? ParametersData { get; set; }
    }
}