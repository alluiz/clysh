using System.Collections.Generic;

namespace Clysh.Helper;

/// <summary>
/// The map for <see cref="Clysh"/>
/// </summary>
/// <typeparam name="TObject">The type of object to map</typeparam>
public class ClyshMap<TObject>: Dictionary<string, TObject> where TObject : IClyshIndexable<string>
{
    /// <summary>
    /// Adds a object to the map
    /// </summary>
    /// <param name="o">The object</param>
    public void Add(TObject o)
    {
        base.Add(o.Id, o);
    }

    /// <summary>
    /// Indicates if map contains the id
    /// </summary>
    /// <param name="id">The id</param>
    /// <returns>The indicator</returns>
    public bool Has(string id)
    {
        return ContainsKey(id);
    }
}