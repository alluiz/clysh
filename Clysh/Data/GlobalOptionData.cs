using System.ComponentModel.DataAnnotations;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618

namespace Clysh.Data;

public class GlobalOptionData: OptionData
{
    /// <summary>
    /// The CLI Commands list
    /// </summary>
    [Required]
    public List<string>? Commands { get; set; }
}