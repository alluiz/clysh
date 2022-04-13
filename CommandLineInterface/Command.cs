namespace CommandLineInterface
{
    public interface ICommand
    {
        Action<ICommand, Options, ICommandLineInterface> Action { get; }
        string Name { get; }
        string Description { get; }
        Options AvailableOptions { get; }
        int Order { get; set; }
        Options SelectedOptions { get; }
        ICommand? Parent { get; set; }

        void ActionHelp(ICommandLineInterface cli);
        ICommand AddCommand(ICommand command);
        ICommand AddOption(string name, string abbreviation, string description);
        ICommand AddOption(string name, string description);
        ICommand AddOption(string name, string abbreviation, string description, Argument[] arguments);
        ICommand AddOption(string name, string description, Argument[] arguments);
        void AddSelectedOption(Option optionSelected);
        ICommand GetCommand(string name);
        Option GetOption(string arg);
        Option GetSelectedOption(string key);
        bool HasOption(string key);
    }

    public class Command : ICommand
    {
        public Action<ICommand, Options, ICommandLineInterface> Action { get; }
        public string Name { get; }
        public string Description { get; }
        public Options AvailableOptions { get; }
        public int Order { get; set; }
        public Options SelectedOptions { get; }
        public ICommand? Parent { get; set; }

        public Dictionary<string, ICommand> Commands;

        private readonly Dictionary<string, string> abbreviationToOption;

        public Command(string name!!, string description!!, Action<ICommand, Options, ICommandLineInterface> action!!)
        {
            this.abbreviationToOption = new Dictionary<string, string>();
            this.Commands = new Dictionary<string, ICommand>();
            this.AvailableOptions = new Options();
            this.SelectedOptions = new Options();

            Name = name;
            Description = description;
            Action = action;
            this.AddOption("help", "Show help on screen");
        }

        public ICommand AddOption(string name, string abbreviation, string description)
        {
            Option option = new Option(name, abbreviation, description);
            this.AvailableOptions.AddOption(option);
            this.abbreviationToOption.Add(abbreviation, option.Name);

            return this;
        }

        public ICommand AddCommand(ICommand command)
        {
            command.Parent = this;
            this.Commands.Add(command.Name, command);
            return this;
        }

        public ICommand AddOption(string name, string description)
        {
            Option option = new Option(name, description);
            this.AvailableOptions.AddOption(option);

            return this;
        }

        public ICommand AddOption(string name, string abbreviation, string description, Argument[] arguments)
        {
            Option option = new Option(name, abbreviation, description, arguments);
            this.AvailableOptions.AddOption(option);
            this.abbreviationToOption.Add(abbreviation, option.Name);

            return this;
        }

        public ICommand AddOption(string name, string description, Argument[] arguments)
        {
            Option option = new Option(name, description, arguments);
            this.AvailableOptions.AddOption(option);

            return this;
        }

        public Option GetOption(string arg)
        {
            try
            {
                return this.AvailableOptions.GetOption(arg);
            }
            catch (System.Exception)
            {
                return this.AvailableOptions.GetOption(this.abbreviationToOption[arg]);
            }
        }


        public Option GetSelectedOption(string key)
        {
            return this.SelectedOptions.GetOption(key);
        }

        public void AddSelectedOption(Option optionSelected)
        {
            this.SelectedOptions.AddOption(optionSelected);
        }

        public ICommand GetCommand(string name)
        {
            if (this.Commands.ContainsKey(name))
                return this.Commands[name];
            else
                throw new InvalidOperationException("Invalid argument: " + name);
        }

        public void ActionHelp(ICommandLineInterface cli)
        {
            ICommand? parent = this.Parent;
            string parentCommands = "";

            while (parent != null)
            {
                parentCommands += parent.Name + " ";
                parent = parent.Parent;
            }

            bool hasCommands = HasCommands();

            cli.PrintWithBreak($"Usage: {parentCommands}{this.Name} [options] {(hasCommands ? "[commands]" : "")}");
            cli.PrintEmptyLine();
            cli.PrintWithBreak(this.Description, true);
            cli.PrintWithBreak("[options]:".PadRight(44) + "Description".PadRight(55) + "Arguments", true);

            foreach (var item in this.AvailableOptions.Map.OrderBy(x => x.Key))
            {
                string args = "";

                if (item.Value.Arguments.Length > 0)
                {
                    for (int i = 0; i < item.Value.Arguments.Length; i++)
                    {
                        args += $"<{item.Value.Arguments[i].Name}:{(item.Value.Arguments[i].Required ? "Required" : "Optional")}> ";
                    }
                }

                cli.PrintWithBreak($"    {(item.Value.Abbreviation == null ? "" : "-" + item.Value.Abbreviation).PadRight(10)}--{item.Key.PadRight(28)}{item.Value.Description.PadRight(55)}{args}");
            }

            cli.PrintEmptyLine();

            if (hasCommands)
            {
                cli.PrintWithBreak("[commands]:", true);

                foreach (var item in this.Commands.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value))
                {
                    if (item.Key != this.Name)
                    {
                        cli.PrintWithBreak($"    {item.Key.PadRight(40)}{item.Value.Description}");
                    }
                }

                cli.PrintEmptyLine();
            }
        }

        public bool HasOption(string key)
        {
            return this.AvailableOptions.HasOption(key) || this.abbreviationToOption.ContainsKey(key);
        }

        private bool HasCommands()
        {
            return this.Commands.Count > 0;
        }
    }
}