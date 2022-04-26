namespace CliSharp
{
    public interface ICliSharpService
    {
        ICliSharpCommand RootCommand { get; }
        ICliSharpView View { get; }
        void Execute(string[] args);
        void ExecuteHelp(ICliSharpCommand command, Exception exception);
        void ExecuteHelp(ICliSharpCommand command);
    }
}