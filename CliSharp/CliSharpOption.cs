using System.Text.RegularExpressions;

namespace CliSharp
{
    public class CliSharpOption : CliSharpIndexable
    {
        public string Description { get; }
        public CliSharpParameters Parameters { get; }
        public string? Shortcut { get; }

        private const int MaxDescription = 50;
        private const int MinDescription = 10;
        
        private const int MinShortcut = 1;
        private const int MaxShortcut = 1;

        public CliSharpOption(string id, string description) : base(id)
        {
            Description = Validate(nameof(description), description, MinDescription, MaxDescription);
            Parameters = CliSharpParameters.Create();
        }

        public CliSharpOption(string id, string description, string? shortcut) : this(id, description)
        {
            const string pattern = @"[a-zA-Z]";

            Regex regex = new(pattern);

            if (!string.IsNullOrEmpty(shortcut) && (shortcut.Length is < MinShortcut or > MaxShortcut || !regex.IsMatch(pattern)))
                throw new ArgumentException($"Invalid shortcut. The shortcut must be null or follow the pattern {pattern} and between {MinShortcut} and {MaxShortcut} chars.", nameof(shortcut));

            Shortcut = shortcut;
            Parameters = CliSharpParameters.Create();
        }

        public CliSharpOption(string id, string description, CliSharpParameters parameters) : this(id, description)
        {
            Parameters = parameters;
        }

        public CliSharpOption(string id, string description, string? shortcut, CliSharpParameters parameters) : this(id, description, shortcut)
        {
            Parameters = parameters;
        }

        private static string Validate(string? field, string? value, int min, int max)
        {
            if (value == null || value.Trim().Length < min || value.Trim().Length > max)
                throw new ArgumentException($"Option {field} must be not null or empty and between {min} and {max} chars.", field);

            return value;
        }
    }
}