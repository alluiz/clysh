using System;
using System.Collections.Generic;
using System.Linq;
using Clysh.Core.Builder;
using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// The command for <see cref="Clysh"/>
/// </summary>
public class ClyshCommand : ClyshSimpleIndexable, IClyshCommand
{
    private const string CommandMustHaveOnlyOneParentCommand = "The command must have only one parent. Command: '{0}'";
    private const string TheOptionAddressMemoryIsAlreadyRelatedToAnotherCommandOption = "The option address memory is already related to another command. Option: '{0}'";
    private const string TheGroupAddressMemoryIsAlreadyRelatedToAnotherCommandOption = "The group address memory is already related to another command. Group: '{0}'";

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
        AddVerboseOption();
    }

    private void AddVerboseOption()
    {
        var builder = new ClyshOptionBuilder();
        var verboseOption = builder
            .Id("verbose", "v")
            .Description("Print verbose log on screen")
            .Build();
            
        AddOption(verboseOption);
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

        VerifyParentRecursivity(subCommand);
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
    
    private void VerifyParentRecursivity(IClyshCommand command)
    {
        var tree = command.Id;
        IClyshCommand? commandCheck = this;

        while (commandCheck != null)
        {
            if (command.Id.Equals(commandCheck.Id))
                throw new InvalidOperationException("Command Error: The command '$0' must not be children of itself: $1"
                    .Replace("$0", command.Id)
                    .Replace("$1", $"{commandCheck.Id}>{tree}")
                );

            tree = $"{commandCheck.Id}>{tree}";
            commandCheck = commandCheck.Parent;
        }
    }
    
    /// <summary>
    /// Adds a group to command
    /// </summary>
    /// <param name="group">The group</param>
    /// <exception cref="ClyshException">The group cannot be related to another command</exception>
    public void AddGroups(ClyshGroup group)
    {
        if (group.Command != null)
            throw new ClyshException(string.Format(TheGroupAddressMemoryIsAlreadyRelatedToAnotherCommandOption, group.Id));
        
        group.Command = this;
        
        Groups.Add(group);
    }
}