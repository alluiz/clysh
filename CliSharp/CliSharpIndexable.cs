using System.Text.RegularExpressions;

namespace CliSharp
{
    public class CliSharpIndexable
    {
        public string Id { get; }

        public CliSharpIndexable(string? id)
        {
            string pattern = @"[a-zA-Z]+\w+";

            Regex regex = new(pattern);

            if (id == null || !regex.IsMatch(id))
                throw new ArgumentException($"Invalid id. The id must follow the pattern: {pattern}", nameof(id));
                
            Id = id;
        }
    }
}