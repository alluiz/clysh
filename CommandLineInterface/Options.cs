namespace CommandLineInterface
{
    public class Options
    {
        public Dictionary<string, Option> Map { get; }

        public Options()
        {
            this.Map = new Dictionary<string, Option>();
        }

        public Option GetOption(string name)
        {
            return this.Map[name];
        }

        public void AddOption(Option option)
        {
            this.Map.Add(option.Name, option);
        }

        public bool HasOption(string name) {
            return this.Map.ContainsKey(name);
        }
    }
}