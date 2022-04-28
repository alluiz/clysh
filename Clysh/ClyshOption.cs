using System;
using System.Text.RegularExpressions;

namespace Clysh
{
    public class ClyshOption : ClyshIndexable
    {
        public string Description { get; }
        public ClyshParameters Parameters { get; }
        public string? Shortcut { get; }

        private const int MaxDescription = 50;
        private const int MinDescription = 10;
        
        private const int MinShortcut = 1;
        private const int MaxShortcut = 1;

        public ClyshOption(string id, string description) : base(id)
        {
            Description = Validate(nameof(description), description, MinDescription, MaxDescription);
            Parameters = ClyshParameters.Create();
        }

        public ClyshOption(string id, string description, string? shortcut) : this(id, description)
        {
            const string pattern = @"[a-zA-Z]";

            Regex regex = new(pattern);

            if (!string.IsNullOrEmpty(shortcut))
            {
                if (shortcut.Length is < MinShortcut or > MaxShortcut || !regex.IsMatch(pattern))
                    throw new ArgumentException($"Invalid shortcut. The shortcut must be null or follow the pattern {pattern} and between {MinShortcut} and {MaxShortcut} chars.",
                    nameof(shortcut));

                if (shortcut is "h")
                    throw new ArgumentException("Shortcut 'h' is reserved to help shortcut.", nameof(shortcut));
            }

            Shortcut = shortcut;
            Parameters = ClyshParameters.Create();
        }

        public ClyshOption(string id, string description, ClyshParameters parameters) : this(id, description)
        {
            Parameters = parameters;
        }

        public ClyshOption(string id, string description, string? shortcut, ClyshParameters parameters) : this(id, description, shortcut)
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