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
    
    /// <summary>
    /// The command action
    /// </summary>
    public Action<IClyshCommand, ClyshMap<ClyshOption>, IClyshView>? Action { get; set; }
    
    /// <summary>
    /// The subcommands of the command
    /// </summary>
    public ClyshMap<IClyshCommand> SubCommands { get; }
    
    /// <summary>
    /// The command groups
    /// </summary>
    public ClyshMap<ClyshGroup> Groups { get; set; }
    
    /// <summary>
    /// The command options
    /// </summary>
    public ClyshMap<ClyshOption> Options { get; }
    
    /// <summary>
    /// The command parent
    /// </summary>
    public IClyshCommand? Parent { get; set; }
    
    /// <summary>
    /// The command execution order
    /// </summary>
    public int Order { get; set; }
    
    /// <summary>
    /// The command description
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// The command status
    /// </summary>
    public bool Executed { get; set; }
    
    /// <summary>
    /// Indicates if command require subcommands to be executed
    /// </summary>
    public bool RequireSubcommand { get; set; }

    /// <summary>
    /// The simple name of command
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// The command constructor
    /// </summary>
    public ClyshCommand()
    {
        Groups = new ClyshMap<ClyshGroup>();
        Options = new ClyshMap<ClyshOption>();
        SubCommands = new ClyshMap<IClyshCommand>();
        shortcutToOptionId = new Dictionary<string, string>();
        Description = string.Empty;
        AddHelpOption();
        AddDebugOption();
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

    /// <summary>
    /// Adds an option to the command
    /// </summary>
    /// <param name="option">The option</param>
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
    
    /// <summary>
    /// Adds a subcommand to the command and mark it as parent
    /// </summary>
    /// <param name="subCommand">The subcommand</param>
    public void AddSubCommand(IClyshCommand subCommand)
    {
        if (subCommand.Parent != null)
            throw new ClyshException(string.Format(CommandMustHaveOnlyOneParentCommand, subCommand.Id));

        ParentRecursivity(subCommand);
        subCommand.Parent = this;
        SubCommands.Add(subCommand);
    }

    /// <summary>
    /// Get an option selected by group
    /// </summary>
    /// <param name="group">The group filter</param>
    /// <returns></returns>
    public ClyshOption? GetOptionFromGroup(ClyshGroup group)
    {
        return Options
            .Values
            .SingleOrDefault(x => x.Group != null && x.Group.Equals(group) && x.Selected);
    }
    
    /// <summary>
    /// Get an option selected by group
    /// </summary>
    /// <param name="groupId">The groupId filter</param>
    /// <returns></returns>
    public ClyshOption? GetOptionFromGroup(string groupId)
    {
        return GetOptionFromGroup(Groups[groupId]);
    }

    /// <summary>
    /// Get an option by arg
    /// </summary>
    /// <param name="arg">The argument</param>
    /// <returns></returns>
    public ClyshOption GetOption(string arg)
    {
        return Options.Has(arg) ? Options[arg] : Options[shortcutToOptionId[arg]];
    }

    /// <summary>
    /// Indicates if the command has the option
    /// </summary>
    /// <param name="key">The option key</param>
    /// <returns>The indicator</returns>
    public bool HasOption(string key)
    {
        return Options.Has(key) || shortcutToOptionId.ContainsKey(key);
    }

    /// <summary>
    /// Indicates if the command has any subcommand
    /// </summary>
    /// <returns>The indicator</returns>
    public bool HasAnySubcommand()
    {
        return SubCommands.Any();
    }

    /// <summary>
    /// Indicates if the command has any subcommand executed
    /// </summary>
    /// <returns>The indicator</returns>
    public bool HasAnySubcommandExecuted()
    {
        return SubCommands.Any(x => x.Value.Executed);
    }

    /// <summary>
    /// Indicates if the command has the subcommand
    /// </summary>
    /// <param name="subCommandId">The subcommand id</param>
    /// <returns>The indicator</returns>
    public bool HasSubcommand(string subCommandId)
    {
        return SubCommands.Has(subCommandId);
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
    
    /// <summary>
    /// Adds a group to command
    /// </summary>
    /// <param name="group">The group</param>
    /// <exception cref="ClyshException">The group cannot be related to another command</exception>
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