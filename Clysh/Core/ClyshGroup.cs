using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// A simple Clysh group representation model
/// </summary>
public class ClyshGroup: ClyshEntity
{
    public List<string> Options { get; }

    internal ClyshGroup(): base(10, 0, 0, ClyshConstants.GroupPattern)
    {
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