using System.Text.RegularExpressions;

namespace CommandLineInterface
{
    public class Parameter : Indexable
    {
        private string? data;
        public string? Data { get { return data; } set { Validate(Id, value, MinLength, MaxLength); this.data = value; } }
        private readonly string? pattern;

        public Regex? Regex { get; }
        public bool Required { get; }
        public int MinLength { get; }
        public int MaxLength { get; }

        public Parameter(string id, int minLength, int maxLength, bool required = true) : base(id)
        {
            MinLength = minLength;
            MaxLength = maxLength;
            Required = required;
        }

        public Parameter(string id, int minLength, int maxLength, bool required, string pattern) : this(id, minLength, maxLength, required)
        {
            this.pattern = pattern;
            Regex = new Regex(pattern);
        }

        private void Validate(string field, string? value, int min, int max)
        {
            if (value == null || value.Trim().Length < min || value.Trim().Length > max)
                throw new ArgumentException($"Parameter {field} must be not null or empty and between {min} and {max} chars.", field);

            if (Regex != null && !Regex.IsMatch(value))
                throw new ArgumentException($"Parameter {field} must match the follow regex pattern: {pattern}.", field);

        }

        public override string ToString()
        {
            return this.Id + ":" + this.Data;
        }
    }
}