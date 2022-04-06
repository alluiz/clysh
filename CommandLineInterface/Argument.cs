namespace CommandLineInterface
{
    public class Argument
    {
        public string Name { get; }
        public string? Value { get; set; }
        public bool Required { get; set; }

        public Argument(string name) {
            Name = name;
            Required = true;
        }

        public Argument(string name, bool required) {
            Name = name;
            Required = required;
        }

        public override string ToString()
        {
            return this.Name + ":" + this.Value;
        }
    }
}