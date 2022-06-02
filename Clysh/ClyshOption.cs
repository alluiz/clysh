using ProjectHelper;

namespace Clysh
{
    public class ClyshOption : ClyshSimpleIndexable
    {
        public string? Description { get; set; }
        public ClyshParameters? Parameters { get; set; }
        public string? Shortcut { get; set; }

        public ClyshOption()
        {
            this.Pattern = @"[a-zA-Z]+\w+";
        }

        public string? GetParameter(string id)
        {
            if (this.Parameters == null)
                throw new ClyshException("Option parameters is null");
            
            return this.Parameters.Get(id).Data;
        }
    }
}