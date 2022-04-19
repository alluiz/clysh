using System.Collections.Generic;

namespace CliSharp.Data
{
    public class CliSharpCommandData
    {
        public string? Id { get; set; }
        public string? Description { get; set; }
        public bool Root { get; set; }
        public List<CliSharpOptionData>? OptionsData { get; set; }
        public List<string>? ChildrenCommandsId { get; set; }
    }
}