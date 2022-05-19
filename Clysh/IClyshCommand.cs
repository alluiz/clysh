using ProjectHelper;

namespace Clysh;

public interface IClyshCommand: IIndexable<string>
{
    Action<Map<ClyshOption>, IClyshView>? Action { get; set; }
    string? Description { get; }
    Map<ClyshOption> AvailableOptions { get; }
    int Order { get; set; }
    Map<ClyshOption> SelectedOptions { get; }
    IClyshCommand? Parent { get; set; }
    Map<ClyshCommand> Children { get; }
    void AddSelectedOption(ClyshOption optionSelected);
    ClyshOption GetOption(string arg);
    bool HasOption(string key);
    bool HasAnyChildren();
    bool HasChild(string arg);
    void AddOption(ClyshOption option);
    void AddChild(ClyshCommand child);
}