namespace Clysh.Data
{
    /// <summary>
    /// Class used to deserialize parameter data from a file
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ClyshParameterData
    {
        /// <summary>
        /// Create a <b>ClyshParameterData</b> object
        /// </summary>
        public ClyshParameterData()
        {
        }

        /// <summary>
        /// Create a <b>ClyshParameterData</b> object
        /// </summary>
        /// <param name="id">The id of parameter</param>
        /// <param name="pattern">The regular expression pattern</param>
        /// <param name="required">Indicates if is a required parameter</param>
        /// <param name="minLength">The minimum length</param>
        /// <param name="maxLength">The maximum length</param>
        public ClyshParameterData(string id, string? pattern, bool required, int minLength, int maxLength)
        {
            Id = id;
            Pattern = pattern;
            Required = required;
            MinLength = minLength;
            MaxLength = maxLength;
        }
        
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
        
    }
}