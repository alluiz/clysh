using System.ComponentModel.DataAnnotations;

namespace Clysh.Data;

public class GlobalOptionData: OptionData
{
    /// <summary>
    /// The CLI Commands list
    /// </summary>
    [Required]
    public List<string>? Commands { get; set; }
}