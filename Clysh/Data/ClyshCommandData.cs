using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
// ReSharper disable All

namespace Clysh.Data;

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
    [Required]
    public string? Id { get; set; }

    /// <summary>
    /// The description
    /// </summary>
    [Required]
    public string? Description { get; set; }

    /// <summary>
    /// Indicates if it is the root command
    /// </summary>
    public bool Root { get; set; }

    /// <summary>
    /// The command options data
    /// </summary>
    public List<ClyshOptionData>? Options { get; set; }

    /// <summary>
    /// The subcommands
    /// </summary>
    public List<string>? SubCommands { get; set; }

    /// <summary>
    /// The groups available for command
    /// </summary>
    public List<string>? Groups { get; set; }

    /// <summary>
    /// Indicates if require subcommand
    /// </summary>
    public bool RequireSubcommand { get; set; }
}