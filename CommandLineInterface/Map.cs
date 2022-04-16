namespace CommandLineInterface
{
    public class Map<T> where T : Indexable
    {
        public Dictionary<string, T> Itens { get; }

        public Map()
        {
            this.Itens = new Dictionary<string, T>();
        }

        public T GetByName(string name)
        {
            return this.Itens[name];
        }

        public Map<T> Add(T o)
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