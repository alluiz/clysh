using Clysh.Helper;

namespace Clysh.Core;

public interface IClyshCommand
{
    Action<IClyshCommand, IClyshView>? Action { get; }
    bool Inputed { get; }
    bool IgnoreParents { get; }
    bool RequireSubcommand { get; }
    ClyshMap<ClyshGroup> Groups { get; }
    ClyshMap<ClyshOption> Options { get; }
    ClyshMap<ClyshCommand> SubCommands { get; }
    Dictionary<string, object> Data { get; }
    ClyshCommand? Parent { get; }
    int Order { get; }
    string Name { get; }

    /// <summary>
    /// The ID text
    /// </summary>
    string Id { get; }

    /// <summary>
    /// The description
    /// </summary>
    string Description { get; }

    ClyshOption? GetOptionFromGroup(string groupId);
    IEnumerable<ClyshOption> GetAvailableOptionsFromGroup(string groupId);
    bool AnySubcommandInputed();
    bool AnySubcommand();
    bool HasOption(string key);
    bool HasSubcommand(string subCommandId);
    ClyshOption GetOption(string arg);
}