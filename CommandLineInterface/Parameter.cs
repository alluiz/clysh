namespace CommandLineInterface
{
    public class Parameter : Indexable
    {
        private string? data;
        public string? Data { get { return data; } set { Validate(Id, value, MinLength, MaxLength); this.data = value; } }
        public bool Required { get; }
        public int MinLength { get; }
        public int MaxLength { get; }

        public Parameter(string id, int minLength, int maxLength, bool required) : base(id)
        {
            MinLength = minLength;
            MaxLength = maxLength;
            Required = required;
        }

        private void Validate(string field, string? value, int min, int max)
        {
            if (value == null || value.Trim().Length < min || value.Trim().Length > max)
                throw new ArgumentException($"Parameter {field} must be not null or empty and between {min} and {max} chars.", field);
        }

        public override string ToString()
        {
            return this.Id + ":" + this.Data;
        }
    }
}