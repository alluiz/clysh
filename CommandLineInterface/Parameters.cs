namespace CommandLineInterface
{
    public class Parameters
    {
        private int lastIndexRetrieved = 0;
        private int lastIndexAdd = 0;

        public Parameter[] Itens { get; private set; }

        public Parameters()
        {
            this.Itens = new Parameter[10];
        }

        public Parameters Add(string name, int minLength, int maxLength, bool required = true)
        {
            Parameter parameter = new(name, minLength, maxLength, required);

            this.Itens[lastIndexAdd] = parameter;

            lastIndexAdd++;

            return this;
        }

        public bool WaitingForRequired()
        {
            return Itens.Any(x => x != null && x.Required && x.Data == null);
        }

        public bool WaitingForAny()
        {
            return Itens.Any(x => x != null && x.Data == null);
        }

        public Parameter Get(string id)
        {
            return this.Itens.Single(x => x != null && x.Id == id);
        }

        public bool Has(string id)
        {
            return Itens.Any(x => x != null && x.Id == id);
        }

        public Parameter Last()
        {
            Parameter p = this.Itens[lastIndexRetrieved];
            lastIndexRetrieved++;

            return p;
        }

        public override string ToString()
        {
            string s = "";

            this.Itens.Where(x => x != null && x.Required).ToList().ForEach(k => s += k.Id + ",");

            if (s.Length > 1)
                s = s[..^1];

            return s;
        }
    }
}