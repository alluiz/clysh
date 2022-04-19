namespace CliSharp.Data
{
    /// <summary>
    /// Class used to deserialize option data from file
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CliSharpOptionData
    {
        /// <summary>
        /// Create a <b>CliSharpOptionData</b> object
        /// </summary>
        public CliSharpOptionData()
        {
        }

        /// <summary>
        /// Create a <b>CliSharpOptionData</b> object
        /// </summary>
        /// <param name="id">The id of option</param>
        /// <param name="description">The description</param>
        /// <param name="shortcut">The CLI shortcut</param>
        /// <param name="parametersData">The option parameters data list</param>
        public CliSharpOptionData(string? id, string? description, string? shortcut, List<CliSharpParameterData>? parametersData)
        {
            Id = id;
            Description = description;
            Shortcut = shortcut;
            ParametersData = parametersData;
        }
        
        /// <summary>
        /// The id of option
        /// </summary>
        public string? Id { get; set; }
        /// <summary>
        /// The description
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// The CLI shortcut
        /// </summary>
        public string? Shortcut { get; set; }
        /// <summary>
        /// The option parameters data list
        /// </summary>
        public List<CliSharpParameterData>? ParametersData { get; set; }
    }
}