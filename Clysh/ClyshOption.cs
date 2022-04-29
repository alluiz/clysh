using System;
using System.Text.RegularExpressions;

namespace Clysh
{
    public class ClyshOption : ClyshIndexable
    {
        public string? Description { get; set; }
        public ClyshParameters Parameters { get; set; }
        public string? Shortcut { get; set; }

        public ClyshOption()
        {
            Parameters = ClyshParameters.Create();
        }

        public string? GetParameter(string id)
        {
            return this.Parameters.Get(id).Data;
        }
    }
}