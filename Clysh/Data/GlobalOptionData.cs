using System.ComponentModel.DataAnnotations;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Clysh.Data;

public class GlobalOptionData: OptionData
{
    /// <summary>
    /// The CLI Commands list
    /// </summary>
    [Required]
    public List<string>? Commands { get; set; }
}