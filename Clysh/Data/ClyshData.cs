using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Clysh.Data;

/// <summary>
/// Class used to deserialize data from file
/// </summary>
public class ClyshData
{
    /// <summary>
    /// Create a <b>ClyshData</b> object
    /// </summary>
    public ClyshData()
    {
    }

    /// <summary>
    /// Create a <b>ClyshData</b> object
    /// </summary>
    /// <param name="title">The CLI Title</param>
    /// <param name="version">The CLI Version</param>
    public ClyshData(string title, string version)
    {
        Title = title;
        Version = version;
    }

    /// <summary>
    /// The CLI Title
    /// </summary>
    [Required]
    public string? Title { get; set; }

    /// <summary>
    /// The CLI Version
    /// </summary>
    [Required]
    public string? Version { get; set; }

    /// <summary>
    /// The CLI Commands list
    /// </summary>
    [Required]
    public List<ClyshCommandData>? Commands { get; set; }

    /// <summary>
    /// The CLI custom messages
    /// </summary>
    public Dictionary<string, string>? Messages { get; set; }
}