using System;
using System.Collections.Generic;

namespace Clysh
{
    public interface IClyshCommand
    {
        Action<ClyshMap<ClyshOption>, IClyshView>? Action { get; set; }
        string Id { get; }
        string Description { get; }
        ClyshMap<ClyshOption> AvailableOptions { get; }
        int Order { get; set; }
        ClyshMap<ClyshOption> SelectedOptions { get; }
        IClyshCommand? Parent { get; set; }
        Dictionary<string, IClyshCommand> Commands { get; }
        IClyshCommand AddCommand(IClyshCommand command);
        IClyshCommand AddOption(string? name, string? description);
        IClyshCommand AddOption(string? name, string? description, string? abbreviation);
        IClyshCommand AddOption(string? name, string? description, string? abbreviation, ClyshParameters parameters);
        IClyshCommand AddOption(string? name, string? description, ClyshParameters parameters);
        void AddSelectedOption(ClyshOption optionSelected);
        IClyshCommand GetCommand(string name);
        ClyshOption GetOption(string arg);
        ClyshOption GetSelectedOption(string key);
        bool HasOption(string key);
        bool HasCommands();
        bool HasCommand(string arg);
    }

    public class ClyshCommand : IClyshCommand
    {
        public Action<ClyshMap<ClyshOption>, IClyshView>? Action { get; set; }
        public string Id { get; }
        public string Description { get; }
        public ClyshMap<ClyshOption> AvailableOptions { get; }
        public int Order { get; set; }
        public ClyshMap<ClyshOption> SelectedOptions { get; }
        public IClyshCommand? Parent { get; set; }

        public Dictionary<string, IClyshCommand> Commands { get; private set; }

        private readonly Dictionary<string, string> shortcutToOption;

        public ClyshCommand(string id)
        {
            Id = id;
        }

        private ClyshCommand(string id, string description, Action<ClyshMap<ClyshOption>, IClyshView> action)
        {
            shortcutToOption = new Dictionary<string, string>();
            Commands = new Dictionary<string, IClyshCommand>();
            AvailableOptions = new ClyshMap<ClyshOption>();
            SelectedOptions = new ClyshMap<ClyshOption>();

            Id = id;
            Description = description;
            Action = action;
            AddOption("help", "Show help on screen", "h");
        }

        private ClyshCommand(string? id, string? description)
        {
            shortcutToOption = new Dictionary<string, string>();
            Commands = new Dictionary<string, IClyshCommand>();
            AvailableOptions = new ClyshMap<ClyshOption>();
            SelectedOptions = new ClyshMap<ClyshOption>();

            Id = id ?? throw new ArgumentNullException(id);
            Description = description ?? throw new ArgumentNullException(description);

            AddOption("help", "Show help on screen", "h");
        }

        public IClyshCommand AddOption(string? id, string? description)
        {
            ClyshOption option = new(id, description);
            AvailableOptions.Add(option);

            return this;
        }

        public IClyshCommand AddOption(string? id, string? description, string? abbreviation)
        {
            if (abbreviation == null)
                throw new ArgumentNullException(abbreviation);

            ClyshOption option = new(id, description, abbreviation);
            AvailableOptions.Add(option);
            shortcutToOption.Add(abbreviation, option.Id);

            return this;
        }

        public IClyshCommand AddOption(string? id, string? description, string? abbreviation, ClyshParameters parameters)
        {
            if (abbreviation == null)
                throw new ArgumentNullException(abbreviation);

            ClyshOption option = new(id, description, abbreviation, parameters);
            AvailableOptions.Add(option);
            shortcutToOption.Add(abbreviation, option.Id);

            return this;
        }

        public IClyshCommand AddOption(string? id, string? description, ClyshParameters parameters)
        {
            ClyshOption option = new(id, description, parameters);
            AvailableOptions.Add(option);

            return this;
        }

        public IClyshCommand AddCommand(IClyshCommand command)
        {
            command.Parent = this;
            Commands.Add(command.Id, command);
            return this;
        }

        public ClyshOption GetOption(string arg)
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
        public ClyshOption GetSelectedOption(string key)
        {
            return SelectedOptions.GetByName(key);
        }

        public void AddSelectedOption(ClyshOption optionSelected)
        {
            SelectedOptions.Add(optionSelected);
        }

        public static IClyshCommand Create(string id, string description, Action<ClyshMap<ClyshOption>, IClyshView> action)
        {
            return new ClyshCommand(id, description, action);
        }

        public static IClyshCommand Create(string? id, string? description)
        {
            return new ClyshCommand(id, description);
        }

        public IClyshCommand GetCommand(string name)
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