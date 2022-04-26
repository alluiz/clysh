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
        public CliSharpData()
        {
        }

        /// <summary>
        /// Create a <b>CliSharpData</b> object
        /// </summary>
        /// <param name="title">The CLI Title</param>
        /// <param name="version">The CLI Version</param>
        public CliSharpData(string title, string version)
        {
            Title = title;
            Version = version;
        }
        
        /// <summary>
        /// Create a <b>CliSharpData</b> object
        /// </summary>
        /// <param name="title">The CLI Title</param>
        /// <param name="version">The CLI Version</param>
        /// <param name="commands">The CLI Commands list</param>
        public CliSharpData(string title, string version, List<CliSharpCommandData> commands) : this(title, version)
        {
            Commands = commands;
        }

        /// <summary>
        /// The CLI Title
        /// </summary>
        public string Title { get; set; } = default!;

        /// <summary>
        /// The CLI Version
        /// </summary>
        public string Version { get; set; } = default!;

        /// <summary>
        /// The CLI Commands list
        /// </summary>
        public List<CliSharpCommandData> Commands { get; set; } = default!;
    }
}