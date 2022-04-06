namespace CommandLineInterface
{
    public class Option
    {
        private const int MaxDescription = 50;
        private const int MinDescription = 10;
        private const int MinName = 3;
        private const int MaxName = 20;
        private const int MinAbbrev = 1;
        private const int MaxAbbrev = 1;

        public Option(string name!!, string abbreviation!!, string description!!)
        {
            Validate(nameof(name), name, MinName, MaxName);
            Validate(nameof(abbreviation), abbreviation, MinAbbrev, MaxAbbrev);
            Validate(nameof(description), description, MinDescription, MaxDescription);

            Name = name;
            Abbreviation = abbreviation;
            Description = description;
            this.Arguments = new Argument[0];
        }

        private static void Validate(string field, string value, int min, int max)
        {
            if (value.Trim().Length < min || value.Trim().Length > max)
                throw new ArgumentException($"Option {field} must be not null or empty and between {min} and {max} chars.", field);
        }

        public Option(string name!!, string description!!)
        {
            Validate(nameof(name), name, MinName, MaxName);
            Validate(nameof(description), description, MinDescription, MaxDescription);

            Name = name;
            Description = description;
            this.Arguments = new Argument[0];
        }

        public Option(string name!!, string abbreviation!!, string description!!, Argument[] arguments!!)
        {
            Validate(nameof(name), name, MinName, MaxName);
            Validate(nameof(abbreviation), abbreviation, MinAbbrev, MaxAbbrev);
            Validate(nameof(description), description, MinDescription, MaxDescription);
            Validate(arguments);

            Name = name;
            Abbreviation = abbreviation;
            Description = description;
            this.Arguments = arguments;
        }

        private void Validate(Argument[] arguments)
        {
            bool hasOptional = false;

            for (int i = 0; i < arguments.Length; i++)
            {
                if (hasOptional && arguments[i].Required)
                    throw new ArgumentException("Optional arguments must be on the final of the array.", "arguments");
                
                hasOptional = !arguments[i].Required;
            }
        }

        public Option(string name!!, string description!!, Argument[] arguments!!)
        {
            Validate(nameof(name), name, MinName, MaxName);
            Validate(nameof(description), description, MinDescription, MaxDescription);

            Name = name;
            Description = description;
            this.Arguments = arguments;
        }

        public string Name { get; }
        public string Description { get; }
        public Argument[] Arguments { get; }
        public string? Abbreviation { get; }
    }
}