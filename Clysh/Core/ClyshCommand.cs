using Clysh.Helper;

namespace Clysh.Core;

public class ClyshCommand : ClyshSimpleIndexable, IClyshCommand
{
    public Action<IClyshCommand, ClyshMap<ClyshOption>, IClyshView>? Action { get; set; }
    public ClyshMap<ClyshCommand> SubCommands { get; }
    public ClyshMap<ClyshGroup> Groups { get; set; }
    public ClyshMap<ClyshOption> Options { get; }
    public IClyshCommand? Parent { get; set; }
    public int Order { get; set; }
    public string? Description { get; set; }
    public bool Executed { get; set; }
    public bool RequireSubcommand { get; set; }

    private readonly Dictionary<string, string> shortcutToOptionId;
        
    public ClyshCommand()
    {
        Groups = new ClyshMap<ClyshGroup>();
        Options = new ClyshMap<ClyshOption>();
        SubCommands = new ClyshMap<ClyshCommand>();
        shortcutToOptionId = new Dictionary<string, string>();
        AddHelpOption();
    }
        
    public void AddOption(ClyshOption option)
    {
        Options.Add(option);

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
        SubCommands.Add(child);
    }

    public ClyshOption? GetOptionFromGroup(string group)
    {
        return Options
            .Values
            .SingleOrDefault(x =>
                x.Group?.Id == group && x.Selected);
    }

    public ClyshOption GetOption(string arg)
    {
        try
        {
            return Options[arg];
        }
        catch (Exception)
        {
            return Options[shortcutToOptionId[arg]];
        }
    }

    public bool HasOption(string key)
    {
        return Options.Has(key) || shortcutToOptionId.ContainsKey(key);
    }

    public bool HasAnyChildren()
    {
        return SubCommands.Any();
    }

    public bool HasAnyChildrenExecuted()
    {
        return SubCommands.Any(x => x.Value.Executed);
    }

    public bool HasChild(string name)
    {
        return SubCommands.Has(name);
    }
}