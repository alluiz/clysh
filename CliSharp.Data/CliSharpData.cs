namespace CliSharp.Data
{
    /// <summary>
    /// Class used to deserialize data from file
    /// </summary>
    public class CliSharpData
    {
        /// <summary>
        /// Create a <b>CliSharpData</b> object
        /// </summary>
        /// <param name="title">The CLI Title</param>
        /// <param name="version">The CLI Version</param>
        public CliSharpData(string? title, string? version)
        {
            this.Title = title;
            this.Version = version;
        }
        
        /// <summary>
        /// Create a <b>CliSharpData</b> object
        /// </summary>
        /// <param name="title">The CLI Title</param>
        /// <param name="version">The CLI Version</param>
        /// <param name="commandsData">The CLI Commands list</param>
        public CliSharpData(string? title, string? version, List<CliSharpCommandData>? commandsData) : this(title, version)
        {
            this.CommandsData = commandsData;
        }
        
        /// <summary>
        /// The CLI Title
        /// </summary>
        public string? Title { get; set; }
        /// <summary>
        /// The CLI Version
        /// </summary>
        public string? Version { get; set; }
        /// <summary>
        /// The CLI Commands list
        /// </summary>
        public List<CliSharpCommandData>? CommandsData { get; set; }
    }
}