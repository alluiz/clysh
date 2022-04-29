using System;
using System.Collections.Generic;

namespace Clysh
{
    public class ClyshCommand : ClyshIndexable, IClyshCommand
    {
        public Action<ClyshMap<ClyshOption>, IClyshView>? Action { get; set; }
        public string? Description { get; set; }
        public ClyshMap<ClyshOption> AvailableOptions { get; }
        public int Order { get; set; }
        public ClyshMap<ClyshOption> SelectedOptions { get; }
        public IClyshCommand? Parent { get; set; }
        public ClyshMap<ClyshCommand> Children { get; }

        private Dictionary<string, string> shortcutToOptionId;
        
        public ClyshCommand()
        {
            AvailableOptions = new ClyshMap<ClyshOption>();
            SelectedOptions = new ClyshMap<ClyshOption>();
            Children = new ClyshMap<ClyshCommand>();
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