namespace CommandLineInterface
{
    public interface ICommandLineInterface
    {
        ICommand RootCommand { get; }
        ICommandLineInterfaceFront Front { get; }
        void Execute(string[] args);
        void ExecuteHelp(ICommand command, Exception exception);
        void ExecuteHelp(ICommand command);
    }
}