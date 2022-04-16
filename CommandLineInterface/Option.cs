namespace CommandLineInterface
{
    public class Option : Indexable
    {
        public string Description { get; }
        public Parameters Parameters { get; }
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
            this.Parameters = Parameters.Create();
        }

        public Option(string id!!, string description!!, string abbreviation!!) : this(id, description)
        {
            Validate(nameof(abbreviation), abbreviation, MinAbbrev, MaxAbbrev);
            Abbreviation = abbreviation;
            this.Parameters = Parameters.Create();
        }

        public Option(string id!!, string description!!, Parameters parameters!!) : this(id, description)
        {
            this.Parameters = parameters;
        }

        public Option(string id!!, string description!!, string abbreviation!!, Parameters parameters!!) : this(id, description, abbreviation)
        {
            this.Parameters = parameters;
        }

        private static void Validate(string field, string value, int min, int max)
        {
            if (value.Trim().Length < min || value.Trim().Length > max)
                throw new ArgumentException($"Option {field} must be not null or empty and between {min} and {max} chars.", field);
        }
    }
}