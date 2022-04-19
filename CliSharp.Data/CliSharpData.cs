namespace CliSharp.Data
{
    public class CliSharpData
    {
        public CliSharpData(string title)
        {
            this.Title = title;
        }

        public string? Title { get; set; }
        public string? Version { get; set; }
        public List<CliSharpCommandData>? CommandsData { get; set; }
    }
}