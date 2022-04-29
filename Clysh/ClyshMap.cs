using System.Collections.Generic;

namespace Clysh
{
    public class ClyshMap<T> where T : ClyshIndexable
    {
        public Dictionary<string, T> Itens { get; }

        public ClyshMap()
        {
            Itens = new Dictionary<string, T>();
        }

        public T Get(string name)
        {
            return Itens[name];
        }

        public ClyshMap<T> Add(T o)
        {
            Itens.Add(o.Id, o);
            return this;
        }

        public bool Has(string name)
        {
            return Itens.ContainsKey(name);
        }
    }
}