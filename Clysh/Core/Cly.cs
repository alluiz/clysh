namespace Clysh.Core;

public class Cly : ICly
{
    public Cly(IClyshCommand command, IClyshView view)
    {
        Command = command;
        View = view;
    }

    public IClyshView View { get; }

    public IClyshCommand Command { get; }
}