using System.ComponentModel.DataAnnotations;
// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618

namespace Clysh.Data;

// ReSharper disable once ClassNeverInstantiated.Global

// This class is used only to deserialize command data from JSON or YAML.
/// <summary>
/// Class used to deserialize command data from file
/// </summary>
public class CommandData
{
    /// <summary>
    /// The id of command
    /// </summary>
    [Required]
    public string Id { get; set; }
    /// <summary>
    /// The description
    /// </summary>
    [Required]
    public string Description { get; set; }

    /// <summary>
    /// Indicates if it is the root command
    /// </summary>
    public bool Root { get; set; }

    /// <summary>
    /// The command options data
    /// </summary>
    public List<OptionData>? Options { get; set; }

    /// <summary>
    /// Indicates if command is abstract
    /// </summary>
    public bool Abstract { get; set; }

    /// <summary>
    /// Indicates if the command must ignore parents execution.
    /// </summary>
    public bool IgnoreParents { get; set; }
}