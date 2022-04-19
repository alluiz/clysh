using CliSharp.Data;

namespace CliSharp
{
    public interface ICliSharpView
    {
        CliSharpData Data { get; set; }
        int PrintedLines { get; }
        bool Confirm(string text = "Do you agree?", string yes = "Y", string no = "n");
        string AskFor(string text, bool sensitive = false);
        string AskForSensitive(string text);
        void Print(string text);
        void PrintEmptyLine();
        void PrintWithBreak(string text, bool emptyLine = false);
        void PrintSeparator();
        void PrintHelp(ICliSharpCommand command);
        void PrintHelp(ICliSharpCommand command, Exception exception);
    }
}