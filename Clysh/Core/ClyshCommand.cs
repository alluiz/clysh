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
    private const string CommandMustHaveOnlyOneParentCommand = "The command must have only one parent. Command: '{0}'";
    private const string TheOptionAddressMemoryIsAlreadyRelatedToAnotherCommandOption = "The option address memory is already related to another command. Option: '{0}'";
    private const string TheGroupAddressMemoryIsAlreadyRelatedToAnotherCommandOption = "The group address memory is already related to another command. Group: '{0}'";
    private const string TheGroupAddressMemoryIsDifferent = "The group address memory is different between command and option. Group: '{0}'";

    private readonly Dictionary<string, string> shortcutToOptionId;

    public ClyshCommand()
    {
        Groups = new ClyshMap<ClyshGroup>();
        Options = new ClyshMap<ClyshOption>();
        SubCommands = new ClyshMap<IClyshCommand>();
        Data = new Dictionary<string, object>();
        shortcutToOptionId = new Dictionary<string, string>();
        Description = string.Empty;
        AddHelpOption();
        AddDebugOption();
    }

    public Dictionary<string, object> Data { get; }

    public Action<IClyshCommand, ClyshMap<ClyshOption>, IClyshView>? Action { get; set; }

    public ClyshMap<IClyshCommand> SubCommands { get; }

    public ClyshMap<ClyshGroup> Groups { get; set; }

    public ClyshMap<ClyshOption> Options { get; }

    public IClyshCommand? Parent { get; set; }

    public int Order { get; set; }

    public string Description { get; set; }

    public bool Executed { get; set; }

    public bool RequireSubcommand { get; set; }

    public string Name { get; set; } = default!;

    public void AddOption(ClyshOption option)
    {
        if (option.Command != null)
            throw new ClyshException(string.Format(TheOptionAddressMemoryIsAlreadyRelatedToAnotherCommandOption, option.Id));

        option.Command = this;

        if (Options.Has(option.Id))
            throw new ClyshException($"Invalid option id. The command already has an option with id: {option.Id}.");
        
        if (option.Shortcut != null && shortcutToOptionId.ContainsKey(option.Shortcut))
            throw new ClyshException($"Invalid option shortcut. The command already has an option with shortcut: {option.Shortcut}.");
        
        if (option.Group != null && !Groups.Has(option.Group.Id))
            AddGroups(option.Group);
        
        Options.Add(option);

        if (option.Shortcut != null)
            shortcutToOptionId.Add(option.Shortcut, option.Id);
    }

    public void AddSubCommand(IClyshCommand subCommand)
    {
        if (subCommand.Parent != null)
            throw new ClyshException(string.Format(CommandMustHaveOnlyOneParentCommand, subCommand.Id));

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
        return Options.Has(arg) ? Options[arg] : Options[shortcutToOptionId[arg]];
    }

    public bool HasOption(string key)
    {
        return Options.Has(key) || shortcutToOptionId.ContainsKey(key);
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

    private void ParentRecursivity(IClyshCommand command)
    {
        var splittedId = command.Id.Split(".");

        if (splittedId.DistinctBy(x => x).Count() != splittedId.Length)
            throw new ClyshException("Command Error: The commandId cannot have duplicated words.");
    }

    public void AddGroups(ClyshGroup group)
    {
        if (group.Command != null && !group.Command.Equals(this))
            throw new ClyshException(string.Format(TheGroupAddressMemoryIsAlreadyRelatedToAnotherCommandOption, group.Id));
        
        group.Command = this;
        
        if (!Groups.Has(group.Id))
            Groups.Add(group);
        else if (!Groups[group.Id].Equals(group))
            throw new ClyshException(string.Format(TheGroupAddressMemoryIsDifferent, group.Id));
    }
}