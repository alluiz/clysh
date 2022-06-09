using System.Collections.Generic;

namespace Clysh.Helper;

public class ClyshMap<TObject>: Dictionary<string, TObject> where TObject : IClyshIndexable<string>
{
    public void Add(TObject o)
    {
        base.Add(o.Id, o);
    }

    public bool Has(string id)
    {
        return ContainsKey(id);
    }
}