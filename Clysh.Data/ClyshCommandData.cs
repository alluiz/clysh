using System.Collections.Generic;

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
        /// Create a <b>ClyshCommandData</b> object
        /// </summary>
        public ClyshCommandData()
        {
        }

        /// <summary>
        /// Create a <b>ClyshCommandData</b> object
        /// </summary>
        /// <param name="id">The id of command</param>
        /// <param name="description">The description</param>
        /// <param name="root">Indicates if this is root command</param>
        /// <param name="options">The command options data</param>
        /// <param name="childrenCommandsId">The children commands</param>
        public ClyshCommandData(string? id, string? description, bool root, List<ClyshOptionData>? options, List<string>? childrenCommandsId)
        {
            Id = id;
            Description = description;
            Root = root;
            Options = options;
            ChildrenCommandsId = childrenCommandsId;
        }

        /// <summary>
        /// The id of command
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The description
        /// </summary>
        public string Description { get; set; }
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
        public List<string>? ChildrenCommandsId { get; set; }
    }
}