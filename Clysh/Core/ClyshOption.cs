using Clysh.Helper;

namespace Clysh.Core;

public class ClyshOption : ClyshSimpleIndexable
{
    public string? Description { get; set; }
    public ClyshParameters Parameters { get; set; }
    public string? Shortcut { get; set; }
    public bool Selected { get; set; }

    public ClyshOption()
    {
        Pattern = @"[a-zA-Z]+\w+";
        Parameters = new ClyshParameters();
    }
}