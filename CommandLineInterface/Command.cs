namespace CommandLineInterface
{
    public interface ICommand
    {
        Action<Map<Option>, ICommandLineInterfaceFront> Action { get; }
        string Name { get; }
        string Description { get; }
        Map<Option> AvailableOptions { get; }
        int Order { get; set; }
        Map<Option> SelectedOptions { get; }
        ICommand? Parent { get; set; }
        Dictionary<string, ICommand> Commands { get; }
        ICommand AddCommand(ICommand command);
        ICommand AddOption(string name, string description);
        ICommand AddOption(string name, string description, string abbreviation);
        ICommand AddOption(string name, string description, Arguments arguments);
        void AddSelectedOption(Option optionSelected);
        ICommand GetCommand(string name);
        Option GetOption(string arg);
        Option GetSelectedOption(string key);
        bool HasOption(string key);
        bool HasCommands();
    }

    public class Command : ICommand
    {
        public Action<Map<Option>, ICommandLineInterfaceFront> Action { get; }
        public string Name { get; }
        public string Description { get; }
        public Map<Option> AvailableOptions { get; }
        public int Order { get; set; }
        public Map<Option> SelectedOptions { get; }
        public ICommand? Parent { get; set; }

        public Dictionary<string, ICommand> Commands { get; private set; }

        private readonly Dictionary<string, string> abbreviationToOption;

        public Command(string name!!, string description!!, Action<Map<Option>, ICommandLineInterfaceFront> action!!)
        {
            this.abbreviationToOption = new Dictionary<string, string>();
            this.Commands = new Dictionary<string, ICommand>();
            this.AvailableOptions = new Map<Option>();
            this.SelectedOptions = new Map<Option>();

            Name = name;
            Description = description;
            Action = action;
            this.AddOption("help", "Show help on screen");
        }

        public ICommand AddOption(string name, string description)
        {
            Option option = new Option(name, description);
            this.AvailableOptions.Add(option);

            return this;
        }

        public ICommand AddOption(string name, string description, string abbreviation)
        {
            Option option = new Option(name, description, abbreviation);
            this.AvailableOptions.Add(option);
            this.abbreviationToOption.Add(abbreviation, option.Id);

            return this;
        }

        public ICommand AddOption(string name, string description, Arguments arguments)
        {
            Option option = new Option(name, description, arguments);
            this.AvailableOptions.Add(option);

            return this;
        }

        public ICommand AddCommand(ICommand command)
        {
            command.Parent = this;
            this.Commands.Add(command.Name, command);
            return this;
        }

        public Option GetOption(string arg)
        {
            try
            {
                return this.AvailableOptions.Get(arg);
            }
            catch (System.Exception)
            {
                return this.AvailableOptions.Get(this.abbreviationToOption[arg]);
            }
        }


        public Option GetSelectedOption(string key)
        {
            return this.SelectedOptions.Get(key);
        }

        public void AddSelectedOption(Option optionSelected)
        {
            this.SelectedOptions.Add(optionSelected);
        }

        public ICommand GetCommand(string name)
        {
            if (this.Commands.ContainsKey(name))
                return this.Commands[name];
            else
                throw new InvalidOperationException("Invalid argument: " + name);
        }

        public bool HasOption(string key)
        {
            return this.AvailableOptions.Has(key) || this.abbreviationToOption.ContainsKey(key);
        }

        public bool HasCommands()
        {
            return this.Commands.Count > 0;
        }
    }
}