using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// The command interface for <see cref="Clysh"/>
/// </summary>
public interface IClyshCommand: IClyshIndexable
{
    /// <summary>
    /// The command data output
    /// </summary>
    Dictionary<string, object> Data { get; }

    /// <summary>
    /// The command action
    /// </summary>
    Action<IClyshCommand, IClyshView>? Action { get; set; }

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
    ClyshMap<ClyshGroup> Groups { get; }

    /// <summary>
    /// The command status
    /// </summary>
    bool Executed { get; set; }

    /// <summary>
    /// Indicates if command require subcommands to be executed
    /// </summary>
    bool RequireSubcommand { get; }

    /// <summary>
    /// The path of command in the tree
    /// </summary>
    string Name { get; }

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

    /// <summary>
    /// Get all options from group
    /// </summary>
    /// <param name="group">The group filter</param>
    /// <returns>The group available options</returns>
    List<ClyshOption> GetAvailableOptionsFromGroup(ClyshGroup group);

    /// <summary>
    /// Get all options from group
    /// </summary>
    /// <param name="groupId">The groupId filter</param>
    /// <returns>The group available options</returns>
    List<ClyshOption> GetAvailableOptionsFromGroup(string groupId);
}