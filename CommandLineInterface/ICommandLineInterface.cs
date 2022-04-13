namespace CommandLineInterface
{
    public interface ICommandLineInterface
    {
        string Title { get; set; }
        ICommand RootCommand { get; }
        bool Confirm(string text = "Do you agree? (Y/n): ", string yes = "Y");
        ICommand CreateCommand(string name, string description, Action<ICommand, Options, ICommandLineInterface> action);
        string AskFor(string text, bool sensitive = false);
        string AskForSensitive(string text);
        void Execute(string[] args);
        void ExecuteHelp(ICommand command, Exception? exception);
        void ExecuteHelp(ICommand command);
        void Print(string text);
        void PrintEmptyLine();
        void PrintWithBreak(string text, bool emptyLine = false);
    }
}