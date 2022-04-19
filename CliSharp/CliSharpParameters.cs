using System.Collections;

namespace CliSharp
{
    public class CliSharpParameters : IEnumerable<CliSharpParameter>
    {
        private int lastIndexRetrieved = 0;
        private int lastIndexAdd = 0;

        public CliSharpParameter[] Itens { get; private set; }

        private CliSharpParameters(CliSharpParameter[] itens)
        {
            this.Itens = itens;
        }

        public static CliSharpParameters Create(params CliSharpParameter[] itens)
        {
            return new CliSharpParameters(itens);
        }

        public void Add(string name, int minLength, int maxLength, bool required = true)
        {
            CliSharpParameter parameter = new(name, minLength, maxLength, required);

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

        public CliSharpParameter Get(string id)
        {
            return this.Itens.Single(x => x.Id == id);
        }

        public bool Has(string id)
        {
            return Itens.Any(x => x.Id == id);
        }

        public CliSharpParameter Last()
        {
            CliSharpParameter p = this.Itens[lastIndexRetrieved];
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
                CliSharpParameter parameter = Itens[i];
                string type = parameter.Required ? "R" : "O";
                paramsText += $"{i}:<{parameter.Id}:{type}>{(i < Itens.Length - 1 ? ", " : "")}";
            }

            if (Itens.Length > 0)
                paramsText = $"[{paramsText}]: {Itens.Length}";

            return paramsText;
        }

        public IEnumerator<CliSharpParameter> GetEnumerator()
        {
            foreach (CliSharpParameter parameter in this.Itens)
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