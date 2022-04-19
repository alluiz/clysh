namespace CliSharp.Data
{
    public class CliSharpParameterData
    {
        public string? Id { get; set; }
        public string? Pattern { get; set;}
        public bool Required { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        
    }
}