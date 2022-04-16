namespace CommandLineInterface
{
    public interface ICommandLineInterface
    {
        ICommand RootCommand { get; }
        ICommand CreateCommand(string name, string description, Action<Map<Option>, ICommandLineInterfaceFront> action);
        void Execute(string[] args);
        void ExecuteHelp(ICommand command, Exception exception);
        void ExecuteHelp(ICommand command);
    }
}