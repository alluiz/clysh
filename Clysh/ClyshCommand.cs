using Clysh.Helper;

namespace Clysh
{
    public class ClyshCommand : ClyshSimpleIndexable, IClyshCommand
    {
        public Action<ClyshMap<ClyshOption>, IClyshView>? Action { get; set; }
        public ClyshMap<ClyshCommand> Children { get; }
        public ClyshMap<ClyshOption> AvailableOptions { get; }
        public ClyshMap<ClyshOption> SelectedOptions { get; }
        public IClyshCommand? Parent { get; set; }
        public int Order { get; set; }
        public string? Description { get; set; }
        
        private readonly Dictionary<string, string> shortcutToOptionId;
        
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
            var builder = new ClyshOptionBuilder();
            var helpOption = builder
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
                return AvailableOptions[arg];
            }
            catch (Exception)
            {
                return AvailableOptions[shortcutToOptionId[arg]];
            }
        }

        public bool HasOption(string key)
        {
            return AvailableOptions.Has(key) || shortcutToOptionId.ContainsKey(key);
        }

        public bool HasAnyChildren()
        {
            return Children.Any();
        }

        public bool HasChild(string name)
        {
            return Children.Has(name);
        }
    }
}