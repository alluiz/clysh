using System;
using System.Collections.Generic;
using System.Linq;
using Clysh.Core.Builder;
using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// The command for <see cref="Clysh"/>
/// </summary>
public class ClyshCommand : ClyshIndexable, IClyshCommand
{
    private const int MaxDescription = 100;
    private const int MinDescription = 10;
    private const string MessageCommandMustHaveOnlyOneParentCommand = "Invalid command: The command must have only one parent. Command: '{0}'";
    private const string MessageOptionAddressMemory = "Invalid command: The option memory address is already related to another command. Option: '{0}'";
    private const string MessageGroupAddressMemory = "Invalid command: The group memory address is already related to another command. Group: '{0}'";
    private const string MessageGroupAddressMemoryIsDifferent = "Invalid command: The group memory address is different between command and option. Group: '{0}'";
    private const string MessageInvalidDescription = "Invalid command: Command description must be not null or empty and between {0} and {1} chars. Description: '{2}'";

    private readonly Dictionary<string, string> _shortcutToOptionId;
    private string _description = string.Empty;

    public ClyshCommand()
    {
        Pattern = @"^[a-z](\.*[a-z]*[0-9]*)+$";
        Groups = new ClyshMap<ClyshGroup>();
        Options = new ClyshMap<ClyshOption>();
        SubCommands = new ClyshMap<IClyshCommand>();
        Data = new Dictionary<string, object>();
        _shortcutToOptionId = new Dictionary<string, string>();
        AddHelpOption();
        AddVersionOption();
        AddDebugOption();
    }

    public Dictionary<string, object> Data { get; }

    public Action<IClyshCommand, IClyshView>? Action { get; set; }

    public ClyshMap<IClyshCommand> SubCommands { get; }

    public ClyshMap<ClyshGroup> Groups { get; set; }

    public ClyshMap<ClyshOption> Options { get; }

    public IClyshCommand? Parent { get; set; }

    public int Order { get; set; }

    /// <summary>
    /// The description
    /// </summary>
    public string Description
    {
        get => _description;
        set => _description = ValidateDescription(value);
    }

    public bool Executed { get; set; }

    public bool RequireSubcommand { get; set; }

    public string Name { get; set; } = default!;

    public void AddOption(ClyshOption option)
    {
        if (option.Command != null)
            throw new ClyshException(string.Format(MessageOptionAddressMemory,
                option.Id));

        option.Command = this;

        if (Options.Has(option.Id))
            throw new ClyshException($"Invalid option id. The command already has an option with id: {option.Id}.");

        if (option.Shortcut != null && _shortcutToOptionId.ContainsKey(option.Shortcut))
            throw new ClyshException(
                $"Invalid option shortcut. The command already has an option with shortcut: {option.Shortcut}.");

        if (option.Group != null && !Groups.Has(option.Group.Id))
            AddGroups(option.Group);

        Options.Add(option);

        if (option.Shortcut != null)
            _shortcutToOptionId.Add(option.Shortcut, option.Id);
    }

    public void AddSubCommand(IClyshCommand subCommand)
    {
        if (subCommand.Parent != null)
            throw new ClyshException(string.Format(MessageCommandMustHaveOnlyOneParentCommand, subCommand.Id));

        ParentRecursivity(subCommand);
        subCommand.Parent = this;
        SubCommands.Add(subCommand);
    }

    public ClyshOption? GetOptionFromGroup(ClyshGroup group)
    {
        return Options
            .Values
            .SingleOrDefault(x => x.Group != null && x.Group.Equals(group) && x.Selected);
    }

    public ClyshOption? GetOptionFromGroup(string groupId)
    {
        return GetOptionFromGroup(Groups[groupId]);
    }

    public List<ClyshOption> GetAvailableOptionsFromGroup(ClyshGroup group)
    {
        return Options
            .Values
            .Where(x => x.Group != null && x.Group.Equals(group))
            .ToList();
    }

    public List<ClyshOption> GetAvailableOptionsFromGroup(string groupId)
    {
        return GetAvailableOptionsFromGroup(Groups[groupId]);
    }

    public ClyshOption GetOption(string arg)
    {
        return Options.Has(arg) ? Options[arg] : Options[_shortcutToOptionId[arg]];
    }

    public bool HasOption(string key)
    {
        return Options.Has(key) || _shortcutToOptionId.ContainsKey(key);
    }

    public bool HasAnySubcommand()
    {
        return SubCommands.Any();
    }

    public bool HasAnySubcommandExecuted()
    {
        return SubCommands.Any(x => x.Value.Executed);
    }

    public bool HasSubcommand(string subCommandId)
    {
        return SubCommands.Has(subCommandId);
    }

    private static string ValidateDescription(string? descriptionValue)
    {
        if (descriptionValue == null || descriptionValue.Trim().Length is < MinDescription or > MaxDescription)
            throw new ArgumentException(
                string.Format(MessageInvalidDescription, MinDescription, MaxDescription, descriptionValue),
                nameof(descriptionValue));

        return descriptionValue;
    }

    private void AddDebugOption()
    {
        var builder = new ClyshOptionBuilder();
        var debugOption = builder
            .Id("debug")
            .Description("Print debug log on screen")
            .Build();

        AddOption(debugOption);
    }

    private void AddHelpOption()
    {
        var builder = new ClyshOptionBuilder();
        var helpOption = builder
            .Id("help", "h")
            .Description("Show help on screen")
            .Build();

        AddOption(helpOption);
    }
    
    private void AddVersionOption()
    {
        var builder = new ClyshOptionBuilder();
        var versionOption = builder
            .Id("version", "v")
            .Description("Show the CLI version")
            .Build();

        AddOption(versionOption);
    }

    private void ParentRecursivity(IClyshCommand command)
    {
        var splittedId = command.Id.Split(".");

        if (splittedId.DistinctBy(x => x).Count() != splittedId.Length)
            throw new ClyshException(
                $"Invalid command: The commandId cannot have duplicated words. Command: {command.Id}");
    }

    public void AddGroups(ClyshGroup group)
    {
        if (group.Command != null && !group.Command.Equals(this))
            throw new ClyshException(string.Format(MessageGroupAddressMemory,
                group.Id));

        group.Command = this;

        if (!Groups.Has(group.Id))
            Groups.Add(group);
        else if (!Groups[group.Id].Equals(group))
            throw new ClyshException(string.Format(MessageGroupAddressMemoryIsDifferent, group.Id));
    }
}