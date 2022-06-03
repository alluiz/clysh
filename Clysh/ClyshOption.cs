using Clysh.Helper;

namespace Clysh
{
    public class ClyshOption : ClyshSimpleIndexable
    {
        public string? Description { get; set; }
        public ClyshParameters Parameters { get; set; }
        public string? Shortcut { get; set; }

        public ClyshOption()
        {
            Pattern = @"[a-zA-Z]+\w+";
            Parameters = ClyshParameters.Create();
        }

        public string? GetParameter(string id)
        {
            if (this.Parameters == null)
                throw new ClyshException("Option parameters is null");
            
            return this.Parameters.Get(id).Data;
        }
    }
}