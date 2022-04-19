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

        private readonly Dictionary<string, string> abbreviationToOption;

        private CliSharpCommand(string id, string description, Action<CliSharpMap<CliSharpOption>, ICliSharpView> action)
        {
            this.abbreviationToOption = new Dictionary<string, string>();
            this.Commands = new Dictionary<string, ICliSharpCommand>();
            this.AvailableOptions = new CliSharpMap<CliSharpOption>();
            this.SelectedOptions = new CliSharpMap<CliSharpOption>();

            Id = id;
            Description = description;
            Action = action;
            this.AddOption("help", "Show help on screen");
        }

        private CliSharpCommand(string? id, string? description)
        {
            this.abbreviationToOption = new Dictionary<string, string>();
            this.Commands = new Dictionary<string, ICliSharpCommand>();
            this.AvailableOptions = new CliSharpMap<CliSharpOption>();
            this.SelectedOptions = new CliSharpMap<CliSharpOption>();

            if (id == null)
                throw new ArgumentNullException(id);

            if (description == null)
                throw new ArgumentNullException(description);

            Id = id;
            Description = description;

            this.AddOption("help", "Show help on screen");
        }

        public ICliSharpCommand AddOption(string? id, string? description)
        {
            CliSharpOption option = new(id, description);
            this.AvailableOptions.Add(option);

            return this;
        }

        public ICliSharpCommand AddOption(string? id, string? description, string? abbreviation)
        {
            if (abbreviation == null)
                throw new ArgumentNullException(abbreviation);

            CliSharpOption option = new(id, description, abbreviation);
            this.AvailableOptions.Add(option);
            this.abbreviationToOption.Add(abbreviation, option.Id);

            return this;
        }

        public ICliSharpCommand AddOption(string? id, string? description, string? abbreviation, CliSharpParameters parameters)
        {
            if (abbreviation == null)
                throw new ArgumentNullException(abbreviation);

            CliSharpOption option = new(id, description, abbreviation, parameters);
            this.AvailableOptions.Add(option);
            this.abbreviationToOption.Add(abbreviation, option.Id);

            return this;
        }

        public ICliSharpCommand AddOption(string? id, string? description, CliSharpParameters parameters)
        {
            CliSharpOption option = new(id, description, parameters);
            this.AvailableOptions.Add(option);

            return this;
        }

        public ICliSharpCommand AddCommand(ICliSharpCommand command)
        {
            command.Parent = this;
            this.Commands.Add(command.Id, command);
            return this;
        }

        public CliSharpOption GetOption(string arg)
        {
            try
            {
                return this.AvailableOptions.GetByName(arg);
            }
            catch (System.Exception)
            {
                return this.AvailableOptions.GetByName(this.abbreviationToOption[arg]);
            }
        }
        public CliSharpOption GetSelectedOption(string key)
        {
            return this.SelectedOptions.GetByName(key);
        }

        public void AddSelectedOption(CliSharpOption optionSelected)
        {
            this.SelectedOptions.Add(optionSelected);
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
            return this.Commands[name];
        }

        public bool HasOption(string key)
        {
            return this.AvailableOptions.Has(key) || this.abbreviationToOption.ContainsKey(key);
        }

        public bool HasCommands()
        {
            return this.Commands.Count > 0;
        }

        public bool HasCommand(string name)
        {
            return this.Commands.ContainsKey(name);
        }
    }
}