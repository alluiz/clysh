using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// The option for <see cref="Clysh"/>
/// </summary>
public class ClyshOption : ClyshSimpleIndexable
{
    /// <summary>
    /// The description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// The parameters
    /// </summary>
    public ClyshParameters Parameters { get; set; }
    
    /// <summary>
    /// The shortcut
    /// </summary>
    public string? Shortcut { get; set; }
    
    /// <summary>
    /// The status of option
    /// </summary>
    public bool Selected { get; set; }
    
    /// <summary>
    /// The group
    /// </summary>
    public ClyshGroup? Group { get; set; }

    /// <summary>
    /// The option constructor
    /// </summary>
    public ClyshOption()
    {
        Pattern = @"[a-zA-Z]+\w+";
        Parameters = new ClyshParameters();
    }
}