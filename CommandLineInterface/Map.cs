namespace CommandLineInterface
{
    public class Map<T> where T : Indexable
    {
        public Dictionary<string, T> Itens { get; }

        public Map()
        {
            this.Itens = new Dictionary<string, T>();
        }

        public T Get(string name)
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

        public int Count()
        {
            return this.Itens.Count;
        }

        public override string ToString()
        {
            string s = "";
            this.Itens.Keys.ToList().ForEach(k => s += k + ",");

            return s;
        }
    }
}