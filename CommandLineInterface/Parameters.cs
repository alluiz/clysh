using System.Collections;

namespace CommandLineInterface
{
    public class Parameters : IEnumerable<Parameter>
    {
        private int lastIndexRetrieved = 0;
        private int lastIndexAdd = 0;

        public Parameter[] Itens { get; private set; }

        private Parameters(Parameter[] itens)
        {
            this.Itens = itens;
        }

        public static Parameters Create(params Parameter[] itens)
        {
            return new Parameters(itens);
        }

        public void Add(string name, int minLength, int maxLength, bool required = true)
        {
            Parameter parameter = new(name, minLength, maxLength, required);

            this.Itens[lastIndexAdd] = parameter;

            lastIndexAdd++;
        }

        public bool WaitingForRequired()
        {
            return Itens.Any(x => x.Required && x.Data == null);
        }

        public bool WaitingForAny()
        {
            return Itens.Any(x => x.Data == null);
        }

        public Parameter Get(string id)
        {
            return this.Itens.Single(x => x.Id == id);
        }

        public bool Has(string id)
        {
            return Itens.Any(x => x.Id == id);
        }

        public Parameter Last()
        {
            Parameter p = this.Itens[lastIndexRetrieved];
            lastIndexRetrieved++;

            return p;
        }

        public string RequiredToString()
        {
            string s = "";

            this.Itens.Where(x => x.Required).ToList().ForEach(k => s += k.Id + ",");

            if (s.Length > 1)
                s = s[..^1];

            return s;
        }

        public override string ToString()
        {
            string paramsText = "";

            for (int i = 0; i < Itens.Length; i++)
            {
                Parameter parameter = Itens[i];
                string type = parameter.Required ? "R" : "O";
                paramsText += $"{i}:<{parameter.Id}:{type}>{(i < Itens.Length - 1 ? ", " : "")}";
            }

            if (Itens.Length > 0)
                paramsText = $"[{paramsText}]: {Itens.Length}";

            return paramsText;
        }

        public IEnumerator<Parameter> GetEnumerator()
        {
            foreach (Parameter parameter in this.Itens)
            {
                yield return parameter;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}