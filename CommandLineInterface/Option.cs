namespace CommandLineInterface
{
    public class Option : Indexable
    {
        public string Description { get; }
        public Arguments Arguments { get; }
        public string? Abbreviation { get; }

        private const int MaxDescription = 50;
        private const int MinDescription = 10;
        private const int MinId = 3;
        private const int MaxId = 20;
        private const int MinAbbrev = 1;
        private const int MaxAbbrev = 1;

        public Option(string id!!, string description!!) : base(id)
        {
            Validate(nameof(id), id, MinId, MaxId);
            Validate(nameof(description), description, MinDescription, MaxDescription);

            Description = description;
            this.Arguments = new Arguments();
        }

        public Option(string id!!, string description!!, string abbreviation!!) : base(id)
        {
            Validate(nameof(id), id, MinId, MaxId);
            Validate(nameof(abbreviation), abbreviation, MinAbbrev, MaxAbbrev);
            Validate(nameof(description), description, MinDescription, MaxDescription);

            Abbreviation = abbreviation;
            Description = description;
            this.Arguments = new Arguments();
        }

        public Option(string id!!, string description!!, Arguments arguments!!) : base(id)
        {
            Validate(nameof(id), id, MinId, MaxId);
            Validate(nameof(description), description, MinDescription, MaxDescription);

            Description = description;
            this.Arguments = arguments;
        }

        private void Validate(string field, string value, int min, int max)
        {
            if (value.Trim().Length < min || value.Trim().Length > max)
                throw new ArgumentException($"Option {field} must be not null or empty and between {min} and {max} chars.", field);
        }
    }
}