using ProjectHelper;

namespace Clysh;

public interface IClyshCommand: IIndexable<string>
{
    Action<ClyshMap<ClyshOption>, IClyshView>? Action { get; set; }
    string? Description { get; }
    ClyshMap<ClyshOption> AvailableOptions { get; }
    int Order { get; set; }
    ClyshMap<ClyshOption> SelectedOptions { get; }
    IClyshCommand? Parent { get; set; }
    ClyshMap<ClyshCommand> Children { get; }
    void AddSelectedOption(ClyshOption optionSelected);
    ClyshOption GetOption(string arg);
    bool HasOption(string key);
    bool HasAnyChildren();
    bool HasChild(string arg);
    void AddOption(ClyshOption option);
    void AddChild(ClyshCommand child);
}