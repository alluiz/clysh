namespace CommandLineInterface
{
    public interface ICommandLineInterfaceFront
    {
        Metadata Metadata { get; set; }
        bool Confirm(string text = "Do you agree?", string yes = "Y", string no = "n");
        string AskFor(string text, bool sensitive = false);
        string AskForSensitive(string text);
        void Print(string text);
        void PrintEmptyLine();
        void PrintWithBreak(string text, bool emptyLine = false);
        void PrintSeparator();
        void PrintHelp(ICommand command);
        void PrintHelp(ICommand command, Exception exception);
    }
}