using Clysh.Data;

namespace Clysh.Core;

public interface IClyshView
{
    ClyshData Data { get; set; }
    int PrintedLines { get; }
    bool Confirm(string text = "Do you agree?", string yes = "Y", string no = "n");
    string AskFor(string text, bool sensitive = false);
    string AskForSensitive(string text);
    void PrintEmpty();
    void Print(string? text, bool emptyLine = false, bool noBreak = false);
    void PrintSeparator();
    void PrintHelp(IClyshCommand command);
    void PrintHelp(IClyshCommand command, Exception exception);
}