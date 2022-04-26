namespace CliSharp
{
    public interface ICliSharpCommand
    {
        Action<CliSharpMap<CliSharpOption>, ICliSharpView>? Action { get; set; }
        string Id { get; }
        string Description { get; }
        CliSharpMap<CliSharpOption> AvailableOptions { get; }
        int Order { get; set; }
        CliSharpMap<CliSharpOption> SelectedOptions { get; }
        ICliSharpCommand? Parent { get; set; }
        Dictionary<string, ICliSharpCommand> Commands { get; }
        ICliSharpCommand AddCommand(ICliSharpCommand command);
        ICliSharpCommand AddOption(string? name, string? description);
        ICliSharpCommand AddOption(string? name, string? description, string? abbreviation);
        ICliSharpCommand AddOption(string? name, string? description, string? abbreviation, CliSharpParameters parameters);
        ICliSharpCommand AddOption(string? name, string? description, CliSharpParameters parameters);
        void AddSelectedOption(CliSharpOption optionSelected);
        ICliSharpCommand GetCommand(string name);
        CliSharpOption GetOption(string arg);
        CliSharpOption GetSelectedOption(string key);
        bool HasOption(string key);
        bool HasCommands();
        bool HasCommand(string arg);
    }

    public class CliSharpCommand : ICliSharpCommand
    {
        public Action<CliSharpMap<CliSharpOption>, ICliSharpView>? Action { get; set; }
        public string Id { get; }
        public string Description { get; }
        public CliSharpMap<CliSharpOption> AvailableOptions { get; }
        public int Order { get; set; }
        public CliSharpMap<CliSharpOption> SelectedOptions { get; }
        public ICliSharpCommand? Parent { get; set; }

        public Dictionary<string, ICliSharpCommand> Commands { get; private set; }

        private readonly Dictionary<string, string> shortcutToOption;

        private CliSharpCommand(string id, string description, Action<CliSharpMap<CliSharpOption>, ICliSharpView> action)
        {
            shortcutToOption = new Dictionary<string, string>();
            Commands = new Dictionary<string, ICliSharpCommand>();
            AvailableOptions = new CliSharpMap<CliSharpOption>();
            SelectedOptions = new CliSharpMap<CliSharpOption>();

            Id = id;
            Description = description;
            Action = action;
            AddOption("help", "Show help on screen");
        }

        private CliSharpCommand(string? id, string? description)
        {
            shortcutToOption = new Dictionary<string, string>();
            Commands = new Dictionary<string, ICliSharpCommand>();
            AvailableOptions = new CliSharpMap<CliSharpOption>();
            SelectedOptions = new CliSharpMap<CliSharpOption>();

            if (id == null)
                throw new ArgumentNullException(id);

            if (description == null)
                throw new ArgumentNullException(description);

            Id = id;
            Description = description;

            AddOption("help", "Show help on screen", "h");
        }

        public ICliSharpCommand AddOption(string? id, string? description)
        {
            CliSharpOption option = new(id, description);
            AvailableOptions.Add(option);

            return this;
        }

        public ICliSharpCommand AddOption(string? id, string? description, string? abbreviation)
        {
            if (abbreviation == null)
                throw new ArgumentNullException(abbreviation);

            CliSharpOption option = new(id, description, abbreviation);
            AvailableOptions.Add(option);
            shortcutToOption.Add(abbreviation, option.Id);

            return this;
        }

        public ICliSharpCommand AddOption(string? id, string? description, string? abbreviation, CliSharpParameters parameters)
        {
            if (abbreviation == null)
                throw new ArgumentNullException(abbreviation);

            CliSharpOption option = new(id, description, abbreviation, parameters);
            AvailableOptions.Add(option);
            shortcutToOption.Add(abbreviation, option.Id);

            return this;
        }

        public ICliSharpCommand AddOption(string? id, string? description, CliSharpParameters parameters)
        {
            CliSharpOption option = new(id, description, parameters);
            AvailableOptions.Add(option);

            return this;
        }

        public ICliSharpCommand AddCommand(ICliSharpCommand command)
        {
            command.Parent = this;
            Commands.Add(command.Id, command);
            return this;
        }

        public CliSharpOption GetOption(string arg)
        {
            try
            {
                return AvailableOptions.GetByName(arg);
            }
            catch (Exception)
            {
                return AvailableOptions.GetByName(shortcutToOption[arg]);
            }
        }
        public CliSharpOption GetSelectedOption(string key)
        {
            return SelectedOptions.GetByName(key);
        }

        public void AddSelectedOption(CliSharpOption optionSelected)
        {
            SelectedOptions.Add(optionSelected);
        }

        public static ICliSharpCommand Create(string id, string description, Action<CliSharpMap<CliSharpOption>, ICliSharpView> action)
        {
            return new CliSharpCommand(id, description, action);
        }

        public static ICliSharpCommand Create(string? id, string? description)
        {
            return new CliSharpCommand(id, description);
        }

        public ICliSharpCommand GetCommand(string name)
        {
            return Commands[name];
        }

        public bool HasOption(string key)
        {
            return AvailableOptions.Has(key) || shortcutToOption.ContainsKey(key);
        }

        public bool HasCommands()
        {
            return Commands.Count > 0;
        }

        public bool HasCommand(string name)
        {
            return Commands.ContainsKey(name);
        }
    }
}