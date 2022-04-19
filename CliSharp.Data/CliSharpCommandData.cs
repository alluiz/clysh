namespace CliSharp.Data
{
    // This class is used only to deserialize command data from JSON or YAML.
    /// <summary>
    /// Class used to deserialize command data from file
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CliSharpCommandData
    {
        /// <summary>
        /// Create a <b>CliSharpCommandData</b> object
        /// </summary>
        /// <param name="id">The id of command</param>
        /// <param name="description">The description</param>
        /// <param name="root">Indicates if this is root command</param>
        /// <param name="optionsData">The command options data</param>
        /// <param name="childrenCommandsId">The children commands</param>
        public CliSharpCommandData(string? id, string? description, bool root, List<CliSharpOptionData>? optionsData, List<string>? childrenCommandsId)
        {
            Id = id;
            Description = description;
            Root = root;
            OptionsData = optionsData;
            ChildrenCommandsId = childrenCommandsId;
        }

        /// <summary>
        /// The id of command
        /// </summary>
        public string? Id { get; set; }
        /// <summary>
        /// The description
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Indicates if it is the root command
        /// </summary>
        public bool Root { get; set; }
        /// <summary>
        /// The command options data
        /// </summary>
        public List<CliSharpOptionData>? OptionsData { get; set; }
        /// <summary>
        /// The children commands
        /// </summary>
        public List<string>? ChildrenCommandsId { get; set; }
    }
}