namespace Clysh.Data
{
    /// <summary>
    /// Class used to deserialize option data from file
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ClyshOptionData
    {
        /// <summary>
        /// Create a <b>ClyshOptionData</b> object
        /// </summary>
        public ClyshOptionData()
        {
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
        public List<ClyshParameterData>? Parameters { get; set; }

        public bool DefaultAtGroup { get; set; }

        public string? Group { get; set; }
    }
}