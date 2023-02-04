using System.ComponentModel.DataAnnotations;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Clysh.Data;

/// <summary>
/// Class used to deserialize option data from file
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class OptionData
{
    /// <summary>
    /// The id of option
    /// </summary>
    [Required]
    public string? Id { get; set; }

    /// <summary>
    /// The description
    /// </summary>
    [Required]
    public string? Description { get; set; }

    /// <summary>
    /// The CLI shortcut
    /// </summary>
    public string? Shortcut { get; set; }

    /// <summary>
    /// The option parameters data list
    /// </summary>
    public List<ParameterData>? Parameters { get; set; }

    /// <summary>
    /// The option group
    /// </summary>
    public string? Group { get; set; }
}