using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// A simple Clysh group representation model
/// </summary>
public class ClyshGroup: ClyshSimpleIndexable
{
    public IClyshCommand Command { get; set; }
}