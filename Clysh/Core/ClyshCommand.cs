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
    }

    /// <summary>
    /// Adds an option to the command
    /// </summary>
    /// <param name="option">The option</param>
    public void AddOption(ClyshOption option)
    {
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
        VerifyParentRecursivity(subCommand);
        subCommand.Parent = this;
        SubCommands.Add(subCommand);
    }

    /// <summary>
    /// Get an option selected by group
    /// </summary>
    /// <param name="group">The group filter</param>
    /// <returns></returns>
    public ClyshOption? GetOptionFromGroup(string group)
    {
        return Options
            .Values
            .SingleOrDefault(x => x.Group?.Id == group && x.Selected);
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
}