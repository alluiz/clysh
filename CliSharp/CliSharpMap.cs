namespace CliSharp
{
    public class CliSharpMap<T> where T : CliSharpIndexable
    {
        public Dictionary<string, T> Itens { get; }

        public CliSharpMap()
        {
            Itens = new Dictionary<string, T>();
        }

        public T GetByName(string name)
        {
            return Itens[name];
        }

        public CliSharpMap<T> Add(T o)
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