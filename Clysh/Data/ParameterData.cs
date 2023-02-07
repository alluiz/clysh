using System.ComponentModel.DataAnnotations;
// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618

namespace Clysh.Data;

/// <summary>
/// Class used to deserialize parameter data from a file
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class ParameterData
{
    /// <summary>
    /// The id of parameter
    /// </summary>
    [Required]
    public string Id { get; set; }

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
    [Required]
    public int MinLength { get; set; }

    /// <summary>
    /// The maximum length
    /// </summary>
    [Required]
    public int MaxLength { get; set; }

    /// <summary>
    /// The order of parameter
    /// </summary>
    [Required]
    public int Order { get; set; }
}