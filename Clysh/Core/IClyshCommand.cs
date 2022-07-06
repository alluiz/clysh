using System;
using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// The command interface for <see cref="Clysh"/>
/// </summary>
public interface IClyshCommand: IClyshIndexable
{
    /// <summary>
    /// The command action
    /// </summary>
    Action<IClyshCommand, ClyshMap<ClyshOption>, IClyshView>? Action { get; set; }
    
    /// <summary>
    /// The command description
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// The command options
    /// </summary>
    ClyshMap<ClyshOption> Options { get; }
    
    /// <summary>
    /// The command execution order
    /// </summary>
    int Order { get; set; }
    
    /// <summary>
    /// The command parent
    /// </summary>
    IClyshCommand? Parent { get; set; }
    
    /// <summary>
    /// The subcommands of the command
    /// </summary>
    ClyshMap<IClyshCommand> SubCommands { get; }
    
    /// <summary>
    /// The command groups
    /// </summary>
    ClyshMap<ClyshGroup> Groups { get; set; }
    
    /// <summary>
    /// The command status
    /// </summary>
    bool Executed { get; set; }
    
    /// <summary>
    /// Indicates if command require subcommands to be executed
    /// </summary>
    bool RequireSubcommand { get; set; }
    
    /// <summary>
    /// Get an option by arg
    /// </summary>
    /// <param name="arg">The argument</param>
    /// <returns></returns>
    ClyshOption GetOption(string arg);
    
    /// <summary>
    /// Indicates if the command has the option
    /// </summary>
    /// <param name="key">The option key</param>
    /// <returns>The indicator</returns>
    bool HasOption(string key);
    
    /// <summary>
    /// Indicates if the command has any subcommand
    /// </summary>
    /// <returns>The indicator</returns>
    bool HasAnySubcommand();
    
    /// <summary>
    /// Indicates if the command has any subcommand executed
    /// </summary>
    /// <returns>The indicator</returns>
    bool HasAnySubcommandExecuted();
    
    /// <summary>
    /// Indicates if the command has the subcommand
    /// </summary>
    /// <param name="subCommandId">The subcommand id</param>
    /// <returns>The indicator</returns>
    bool HasSubcommand(string subCommandId);
    
    /// <summary>
    /// Adds an option to the command
    /// </summary>
    /// <param name="option">The option</param>
    void AddOption(ClyshOption option);
    
    /// <summary>
    /// Adds a subcommand to the command and mark it as parent
    /// </summary>
    /// <param name="subCommand">The subcommand</param>
    void AddSubCommand(IClyshCommand subCommand);
    
    /// <summary>
    /// Get an option selected by group
    /// </summary>
    /// <param name="group">The group filter</param>
    /// <returns>The selected option</returns>
    ClyshOption? GetOptionFromGroup(ClyshGroup group);
    
    /// <summary>
    /// Get an option selected by group
    /// </summary>
    /// <param name="groupId">The groupId filter</param>
    /// <returns>The selected option</returns>
    ClyshOption? GetOptionFromGroup(string groupId);
}