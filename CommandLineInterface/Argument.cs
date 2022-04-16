namespace CommandLineInterface
{
    public class Argument: Indexable
    {
        public string? Value { get; set; }
        public bool Required { get; set; }

        public Argument(string id, bool required = true): base(id) {
            Required = required;
        }

        public override string ToString()
        {
            return this.Id + ":" + this.Value;
        }
    }
}