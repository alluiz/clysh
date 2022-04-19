namespace CliSharp
{
    public class CliSharpMap<T> where T : CliSharpIndexable
    {
        public Dictionary<string, T> Itens { get; }

        public CliSharpMap()
        {
            this.Itens = new Dictionary<string, T>();
        }

        public T GetByName(string name)
        {
            return this.Itens[name];
        }

        public CliSharpMap<T> Add(T o)
        {
            this.Itens.Add(o.Id, o);
            return this;
        }

        public bool Has(string name)
        {
            return this.Itens.ContainsKey(name);
        }
    }
}