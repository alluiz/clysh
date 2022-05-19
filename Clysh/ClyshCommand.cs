using System;
using System.Collections.Generic;
using ProjectHelper;

namespace Clysh
{
    public class ClyshCommand : SimpleIndexable, IClyshCommand
    {
        public Action<Map<ClyshOption>, IClyshView>? Action { get; set; }
        public string? Description { get; set; }
        public Map<ClyshOption> AvailableOptions { get; }
        public int Order { get; set; }
        public Map<ClyshOption> SelectedOptions { get; }
        public IClyshCommand? Parent { get; set; }
        public Map<ClyshCommand> Children { get; }

        private readonly Dictionary<string, string> shortcutToOptionId;
        
        public ClyshCommand()
        {
            AvailableOptions = new Map<ClyshOption>();
            SelectedOptions = new Map<ClyshOption>();
            Children = new Map<ClyshCommand>();
            shortcutToOptionId = new Dictionary<string, string>();
            AddHelpOption();
        }
        
        public void AddOption(ClyshOption option)
        {
            AvailableOptions.Add(option);

            if (option.Shortcut != null)
                shortcutToOptionId.Add(option.Shortcut, option.Id);
        }

        private void AddHelpOption()
        {
            ClyshOptionBuilder builder = new ClyshOptionBuilder();
            ClyshOption helpOption = builder
                .Id("help")
                .Description("Show help on screen")
                .Shortcut("h")
                .Build();
            
            AddOption(helpOption);
        }
        
        public void AddChild(ClyshCommand child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        public void AddSelectedOption(ClyshOption optionSelected)
        {
            SelectedOptions.Add(optionSelected);
        }

        public ClyshOption GetOption(string arg)
        {
            try
            {
                return AvailableOptions.Get(arg);
            }
            catch (Exception e)
            {
                return AvailableOptions.Get(shortcutToOptionId[arg]);
            }
        }

        public bool HasOption(string key)
        {
            return AvailableOptions.Has(key) || shortcutToOptionId.ContainsKey(key);
        }

        public bool HasAnyChildren()
        {
            return Children.Itens.Any();
        }

        public bool HasChild(string name)
        {
            return Children.Has(name);
        }
    }
}