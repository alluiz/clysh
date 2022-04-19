using System.Text.RegularExpressions;

namespace CommandLineInterface
{
    public class Indexable
    {
        public string Id { get; }

        public Indexable(string? id)
        {
            string pattern = @"[a-zA-Z]+\w+";

            Regex regex = new(pattern);

            if (id == null || !regex.IsMatch(id))
                throw new ArgumentException($"Invalid id. The id must follow the pattern: {pattern}", nameof(id));
                
            Id = id;
        }
    }
}