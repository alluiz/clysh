using Clysh.Helper;

namespace Clysh.Core;

public interface IClyshCommand: IClyshIndexable<string>
{
    Action<ClyshMap<ClyshOption>, IClyshView>? Action { get; set; }
    string? Description { get; }
    ClyshMap<ClyshOption> Options { get; }
    int Order { get; set; }
    IClyshCommand? Parent { get; set; }
    ClyshMap<ClyshCommand> Children { get; }
    ClyshOption GetOption(string arg);
    bool HasOption(string key);
    bool HasAnyChildren();
    bool HasChild(string arg);
    void AddOption(ClyshOption option);
    void AddChild(ClyshCommand child);
}