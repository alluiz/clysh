using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Clysh.Helper;

/// <summary>
/// The map for <see cref="Clysh"/>
/// </summary>
/// <typeparam name="TObject">The type of object to map</typeparam>
[Serializable]
public class ClyshMap<TObject>: Dictionary<string, TObject> where TObject : IClyshIndexable
{
    /// <summary>
    /// Serialization constructor
    /// </summary>
    /// <param name="info">The serialization info</param>
    /// <param name="context">The streaming context</param>
    protected ClyshMap(SerializationInfo info,
        StreamingContext context): base(info, context)
    {
        
    }

    /// <summary>
    /// Create an empty ClyshMap
    /// </summary>
    public ClyshMap()
    {
        
    }
    
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