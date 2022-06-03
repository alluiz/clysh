using Clysh.Helper;

namespace Clysh
{
    public class ClyshParameters: ClyshMap<ClyshParameter>
    {
        public bool WaitingForRequired()
        {
            return Values.Any(x => x.Required && x.Data == null);
        }

        public bool WaitingForAny()
        {
            return Values.Any(x => x.Data == null);
        }

        public ClyshParameter Last()
        {
            return this.LastOrDefault().Value;
        }

        public string RequiredToString()
        {
            var s = "";

            Values.Where(x => x.Required).ToList().ForEach(k => s += k.Id + ",");

            if (s.Length > 1)
                s = s[..^1];

            return s;
        }

        public override string ToString()
        {
            var paramsText = "";
            var i = 0;
            
            foreach (var parameter in Values)
            {
                var type = parameter.Required ? "R" : "O";
                paramsText += $"{i}:<{parameter.Id}:{type}>{(i < Count - 1 ? ", " : "")}";
                i++;
            }

            if (Count > 0)
                paramsText = $"[{paramsText}]: {Count}";

            return paramsText;
        }

        public static ClyshParameters Create(params ClyshParameter[] array)
        {
            var parameters = new ClyshParameters();
            
            foreach (var parameter in array)
            {
                parameters.Add(parameter);
            }

            return parameters;
        }
    }
}