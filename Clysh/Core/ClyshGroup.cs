using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// A simple Clysh group representation model
/// </summary>
public class ClyshGroup: ClyshIndexable
{
    /// <summary>
    /// The command of group
    /// </summary>
    public IClyshCommand? Command { get; set; }

    public List<string> Options { get; }

    public ClyshGroup()
    {
        MaxLength = 10;
        Options = new List<string>();
    }

    /// <summary>
    /// Group in string format
    /// </summary>
    /// <returns>Group string format</returns>
    public override string ToString()
    {
        return Id;
    }
}