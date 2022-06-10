using System;
using Clysh.Helper;

namespace Clysh.Core;

public interface IClyshCommand: IClyshIndexable<string>
{
    Action<IClyshCommand, ClyshMap<ClyshOption>, IClyshView>? Action { get; set; }
    string? Description { get; }
    ClyshMap<ClyshOption> Options { get; }
    int Order { get; set; }
    IClyshCommand? Parent { get; set; }
    ClyshMap<IClyshCommand> SubCommands { get; }
    ClyshMap<ClyshGroup> Groups { get; set; }
    bool Executed { get; set; }
    bool RequireSubcommand { get; set; }
    ClyshOption GetOption(string arg);
    bool HasOption(string key);
    bool HasAnySubcommand();
    bool HasAnySubcommandExecuted();
    bool HasSubcommand(string subCommandId);
    void AddOption(ClyshOption option);
    void AddSubCommand(IClyshCommand subCommand);
    ClyshOption? GetOptionFromGroup(string group);
}