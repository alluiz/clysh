using Clysh.Core.Builder;
using Clysh.Helper;

// ReSharper disable ClassNeverInstantiated.Global

namespace Clysh.Core;

/// <summary>
/// The command for <see cref="Clysh"/>
/// </summary>
public class ClyshCommand : ClyshEntity
{
    internal ClyshCommand(): base(100, ClyshConstants.CommandPattern, 10, 100)
    {
        Groups = new ClyshMap<ClyshGroup>();
        Options = new ClyshMap<ClyshOption>();
        SubCommands = new ClyshMap<ClyshCommand>();
        Data = new Dictionary<string, object>();
        _shortcuts = new Dictionary<string, ClyshOption>();
        AddHelpOption();
        AddVersionOption();
        AddDebugOption();
    }

    private readonly Dictionary<string, ClyshOption> _shortcuts;
    private Action<ClyshCommand, IClyshView>? _action;

    #region Properties

    public Action<ClyshCommand, IClyshView>? Action
    {
        get => _action;
        internal set
        {
            if (_action != null)
                throw new ArgumentException("The command already has a configured action.");
            
            _action = value;
        }
    }
    public bool Executed { get; internal set; }
    public bool RequireSubcommand { get; internal set; }
    public ClyshMap<ClyshGroup> Groups { get; }
    public ClyshMap<ClyshOption> Options { get; }
    public ClyshMap<ClyshCommand> SubCommands { get; }
    public Dictionary<string, object> Data { get; }
    public ClyshCommand? Parent { get; private set; }
    public int Order { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    
    #endregion
    
    #region Internal Methods
    
    internal void AddOption(ClyshOption option)
    {
        if (!option.IsGlobal)
        {
            if (option.Command != null)
                throw new EntityException(string.Format(ClyshMessages.ErrorOnValidateCommandPropertyMemory,
                    option.Id));
            
            option.Command = this;
        }

        AddSimpleOption(option);
    }
    internal void AddSubCommand(ClyshCommand subCommand)
    {
        if (subCommand.Parent != null)
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnValidateCommandParent, subCommand.Id));

        var splittedId = subCommand.Id.Split(".", StringSplitOptions.RemoveEmptyEntries);

        if (splittedId.DistinctBy(x => x).Count() != splittedId.Length)
            throw new ClyshException(ClyshMessages.ErrorOnValidateCommandId);
        
        subCommand.Parent = this;
        SubCommands.Add(subCommand);
    }

    #endregion

    #region Private Methods
    
    private ClyshOption? GetOptionFromGroup(ClyshGroup group)
    {
        return Options
            .Values
            .SingleOrDefault(option => option.Group != null && option.Group.Equals(group) && option.Selected);
    }
    private List<ClyshOption> GetAvailableOptionsFromGroup(ClyshGroup group)
    {
        return Options
            .Values
            .Where(x => x.Group != null && x.Group.Equals(group))
            .ToList();
    }
    private void AddDebugOption() => AddDefaultOption("debug", "Print debug log on screen");
    private void AddDefaultOption(string id, string description, string? shortcut = null)
    {
        var builder = new ClyshOptionBuilder();
        var versionOption = builder
            .Id(id, shortcut)
            .Description(description)
            .Build();

        AddOption(versionOption);
    }
    private void AddHelpOption() => AddDefaultOption("help", "Show help on screen", "h");
    private void AddSimpleOption(ClyshOption option)
    {
        if (Options.Has(option.Id))
            throw new EntityException($"Invalid option id. The command already has an option with id: {option.Id}.");

        if (option.Shortcut != null && _shortcuts.ContainsKey(option.Shortcut))
            throw new EntityException(
                $"Invalid option shortcut. The command already has an option with shortcut: {option.Shortcut}.");

        if (option.Group != null && !Groups.Has(option.Group.Id))
            AddGroup(option.Group);

        Options.Add(option);

        if (option.Shortcut != null)
            _shortcuts.Add(option.Shortcut, option);
    }
    private void AddVersionOption() => AddDefaultOption("version", "Show the CLI version", "v");
    private void AddGroup(ClyshGroup group)
    {
        if (!Groups.Has(group.Id))
            Groups.Add(group);
        else if (!Groups[group.Id].Equals(group))
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnValidateCommandGroupDuplicated, group.Id));
    }
    private void ValidateSubcommands()
    {
        if (!RequireSubcommand)
            return;

        if (!AnySubcommand())
            throw new EntityException(string.Format(ClyshMessages.ErrorOnValidateCommandSubcommands, Id));
    }
    
    #endregion

    #region Public Methods
    
    public bool AnySubcommandExecuted()
    {
        return SubCommands.Any(x => x.Value.Executed);
    }
    public bool AnySubcommand()
    {
        return SubCommands.Any();
    }
    public bool HasOption(string key)
    {
        return Options.Has(key) || _shortcuts.ContainsKey(key);
    }
    public bool HasSubcommand(string subCommandId)
    {
        return SubCommands.Has(subCommandId);
    }
    public ClyshOption GetOption(string arg)
    {
        return Options.Has(arg) ? Options[arg] : _shortcuts[arg];
    }
    public ClyshOption? GetOptionFromGroup(string groupId)
    {
        if (!Groups.Has(groupId))
            throw new ArgumentException(string.Format(ClyshMessages.ErrorOnGetOptionFromGroupNotFound, groupId), nameof(groupId));
            
        return GetOptionFromGroup(Groups[groupId]);
    }
    public IEnumerable<ClyshOption> GetAvailableOptionsFromGroup(string groupId)
    {
        return GetAvailableOptionsFromGroup(Groups[groupId]);
    }
    internal override void Validate()
    {
        base.Validate();
        ValidateSubcommands();
    }

    #endregion
}