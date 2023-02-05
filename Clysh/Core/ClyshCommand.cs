using Clysh.Core.Builder;
using Clysh.Helper;
using Microsoft.VisualBasic;

// ReSharper disable ClassNeverInstantiated.Global

namespace Clysh.Core;

/// <summary>
/// The command for <see cref="Clysh"/>
/// </summary>
public class ClyshCommand : ClyshEntity, IClyshCommand
{
    private readonly Dictionary<string, IClyshOption> _shortcuts;

    internal ClyshCommand(): base(100, ClyshConstants.CommandPattern, 10, 100)
    {
        Groups = new ClyshMap<ClyshGroup>();
        Options = new ClyshMap<IClyshOption>();
        SubCommands = new ClyshMap<IClyshCommand>();
        Data = new Dictionary<string, object>();
        _shortcuts = new Dictionary<string, IClyshOption>();
        AddHelpOption();
        AddVersionOption();
        AddDebugOption();
    }

    public Dictionary<string, object> Data { get; }

    public Action<IClyshCommand, IClyshView>? Action { get; set; }

    public ClyshMap<IClyshCommand> SubCommands { get; }

    public ClyshMap<ClyshGroup> Groups { get; set; }

    public ClyshMap<IClyshOption> Options { get; }

    public IClyshCommand? Parent { get; set; }

    public int Order { get; set; }

    public bool Executed { get; set; } = false;

    public bool RequireSubcommand { get; set; }

    public string Name { get; set; } = string.Empty;

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

    private void AddSimpleOption(IClyshOption option)
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

    internal void AddSubCommand(IClyshCommand subCommand)
    {
        if (subCommand.Parent != null)
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnValidateCommandParent, subCommand.Id));

        var splittedId = subCommand.Id.Split(".", StringSplitOptions.RemoveEmptyEntries);

        if (splittedId.DistinctBy(x => x).Count() != splittedId.Length)
            throw new ClyshException(ClyshMessages.ErrorOnValidateCommandId);
        
        subCommand.Parent = this;
        SubCommands.Add(subCommand);
    }

    public IClyshOption? GetOptionFromGroup(ClyshGroup group)
    {
        return Options
            .Values
            .SingleOrDefault(option => option.Group != null && option.Group.Equals(group) && option.Selected);
    }

    public IClyshOption? GetOptionFromGroup(string groupId)
    {
        if (!Groups.Has(groupId))
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnGetOptionFromGroupNotFound, groupId));
            
        return GetOptionFromGroup(Groups[groupId]);
    }

    public List<IClyshOption> GetAvailableOptionsFromGroup(ClyshGroup group)
    {
        return Options
            .Values
            .Where(x => x.Group != null && x.Group.Equals(group))
            .ToList();
    }

    public List<IClyshOption> GetAvailableOptionsFromGroup(string groupId)
    {
        return GetAvailableOptionsFromGroup(Groups[groupId]);
    }

    public IClyshOption GetOption(string arg)
    {
        return Options.Has(arg) ? Options[arg] : _shortcuts[arg];
    }

    public bool HasOption(string key)
    {
        return Options.Has(key) || _shortcuts.ContainsKey(key);
    }

    public bool AnySubcommand()
    {
        return SubCommands.Any();
    }

    public bool AnySubcommandExecuted()
    {
        return SubCommands.Any(x => x.Value.Executed);
    }

    public bool HasSubcommand(string subCommandId)
    {
        return SubCommands.Has(subCommandId);
    }

    private void AddDebugOption() => AddDefaultOption("debug", "Print debug log on screen");
    private void AddHelpOption() => AddDefaultOption("help", "Show help on screen", "h");
    private void AddVersionOption() => AddDefaultOption("version", "Show the CLI version", "v");
    private void AddDefaultOption(string id, string description, string? shortcut = null)
    {
        var builder = new ClyshOptionBuilder();
        var versionOption = builder
            .Id(id, shortcut)
            .Description(description)
            .Build();

        AddOption(versionOption);
    }

    private void AddGroup(ClyshGroup group)
    {
        if (!Groups.Has(group.Id))
            Groups.Add(group);
        else if (!Groups[group.Id].Equals(group))
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnValidateCommandGroupDuplicated, group.Id));
    }

    public override void Validate()
    {
        base.Validate();
        ValidateSubcommands();
    }

    private void ValidateSubcommands()
    {
        if (!RequireSubcommand)
            return;

        if (!AnySubcommand())
            throw new EntityException(string.Format(ClyshMessages.ErrorOnValidateCommandSubcommands, Id));
    }
}