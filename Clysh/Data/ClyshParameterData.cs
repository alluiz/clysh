namespace Clysh.Data
{
    /// <summary>
    /// Class used to deserialize parameter data from a file
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ClyshParameterData
    {
        /// <summary>
        /// The id of parameter
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// The regular expression pattern
        /// </summary>
        public string? Pattern { get; set;}
        /// <summary>
        /// Indicates if is a required parameter
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// The minimum length
        /// </summary>
        public int MinLength { get; set; }
        /// <summary>
        /// The maximum length
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        /// The order of parameter
        /// </summary>
        public int Order { get; set; }
    }
}