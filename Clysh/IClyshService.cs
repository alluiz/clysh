namespace Clysh
{
    public interface IClyshService
    {
        IClyshCommand RootCommand { get; }
        IClyshView View { get; }
        void Execute(string[] args);
        void ExecuteHelp(IClyshCommand command, Exception exception);
        void ExecuteHelp(IClyshCommand command);
    }
}