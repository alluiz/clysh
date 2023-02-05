using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// A simple Clysh group representation model
/// </summary>
public class ClyshGroup: ClyshIndexable
{
    public List<string> Options { get; }

    public ClyshGroup()
    {
        maxLength = 10;
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