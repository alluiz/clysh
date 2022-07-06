using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// A simple Clysh group representation model
/// </summary>
public class ClyshGroup: ClyshSimpleIndexable
{
    /// <summary>
    /// The command of group
    /// </summary>
    public IClyshCommand? Command { get; set; }
}