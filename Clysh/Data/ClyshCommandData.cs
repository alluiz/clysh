namespace Clysh.Data
{
    // This class is used only to deserialize command data from JSON or YAML.
    /// <summary>
    /// Class used to deserialize command data from file
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ClyshCommandData
    {
        /// <summary>
        /// The id of command
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// The description
        /// </summary>
        public string Description { get; set; } = null!;

        /// <summary>
        /// Indicates if it is the root command
        /// </summary>
        public bool Root { get; set; }
        /// <summary>
        /// The command options data
        /// </summary>
        public List<ClyshOptionData>? Options { get; set; }
        /// <summary>
        /// The children commands
        /// </summary>
        public List<string>? Children { get; set; }
    }
}