namespace Clysh
{
    public class ClyshParameters
    {
        private int lastIndexRetrieved;

        public ClyshParameter[] Itens { get; }

        private ClyshParameters(ClyshParameter[] itens)
        {
            Itens = itens;
        }

        public static ClyshParameters Create(params ClyshParameter[] itens)
        {
            return new ClyshParameters(itens);
        }

        public bool WaitingForRequired()
        {
            return Itens.Any(x => x.Required && x.Data == null);
        }

        public bool WaitingForAny()
        {
            return Itens.Any(x => x.Data == null);
        }

        public ClyshParameter Get(string id)
        {
            return Itens.Single(x => x.Id == id);
        }

        public bool Has(string id)
        {
            return Itens.Any(x => x.Id == id);
        }

        public ClyshParameter Last()
        {
            var p = Itens[lastIndexRetrieved];
            lastIndexRetrieved++;

            return p;
        }

        public string RequiredToString()
        {
            var s = "";

            Itens.Where(x => x.Required).ToList().ForEach(k => s += k.Id + ",");

            if (s.Length > 1)
                s = s[..^1];

            return s;
        }

        public override string ToString()
        {
            var paramsText = "";

            for (var i = 0; i < Itens.Length; i++)
            {
                var parameter = Itens[i];
                var type = parameter.Required ? "R" : "O";
                paramsText += $"{i}:<{parameter.Id}:{type}>{(i < Itens.Length - 1 ? ", " : "")}";
            }

            if (Itens.Length > 0)
                paramsText = $"[{paramsText}]: {Itens.Length}";

            return paramsText;
        }
    }
}