using System.Text.RegularExpressions;

namespace CommandLineInterface
{
    public class Option : Indexable
    {
        public string Description { get; }
        public Parameters Parameters { get; }
        public string? Abbreviation { get; }

        private const int MaxDescription = 50;
        private const int MinDescription = 10;
        private const int MinAbbrev = 1;
        private const int MaxAbbrev = 1;

        public Option(string? id, string? description) : base(id)
        {
            Description = Validate(nameof(description), description, MinDescription, MaxDescription);
            this.Parameters = Parameters.Create();
        }

        public Option(string? id, string? description, string? shortcut) : this(id, description)
        {
            string pattern = @"[a-z]";

            Regex regex = new(pattern);

            if (!string.IsNullOrEmpty(shortcut) && (string.IsNullOrWhiteSpace(shortcut) || shortcut.Length > 1 || !regex.IsMatch(pattern)))
                throw new ArgumentException($"Invalid shortcut. The shortcut must be null or follow the pattern: {pattern}", nameof(shortcut));

            Validate(nameof(shortcut), shortcut, MinAbbrev, MaxAbbrev);
            Abbreviation = shortcut;
            this.Parameters = Parameters.Create();
        }

        public Option(string? id, string? description, Parameters parameters) : this(id, description)
        {
            this.Parameters = parameters;
        }

        public Option(string? id, string? description, string? abbreviation, Parameters parameters) : this(id, description, abbreviation)
        {
            this.Parameters = parameters;
        }

        private static string Validate(string? field, string? value, int min, int max)
        {
            if (value == null || value.Trim().Length < min || value.Trim().Length > max)
                throw new ArgumentException($"Option {field} must be not null or empty and between {min} and {max} chars.", field);

            return value;
        }
    }
}