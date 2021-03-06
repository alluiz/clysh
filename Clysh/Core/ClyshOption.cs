using System;
using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// The option for <see cref="Clysh"/>
/// </summary>
public class ClyshOption : ClyshIndexable
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
    /// The command owner
    /// </summary>
    public IClyshCommand? Command { get; set; }

    /// <summary>
    /// The option constructor
    /// </summary>
    public ClyshOption()
    {
        Pattern = @"[a-zA-Z]+\w+";
        Parameters = new ClyshParameters();
    }

    /// <summary>
    /// Check the optionId
    /// </summary>
    /// <param name="id">The id to be checked</param>
    /// <returns>The result of validation</returns>
    public bool Is(string id)
    {
        return Id.Equals(id, StringComparison.CurrentCultureIgnoreCase);
    }
}