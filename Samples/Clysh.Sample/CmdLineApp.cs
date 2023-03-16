using Clysh.Core;

namespace Clysh.Sample;

public abstract class CmdLineApp
{
    private IQuickClysh? _cli;

    protected abstract IQuickClysh GetCli(); 

    public void Execute(IEnumerable<string> args)
    {
        _cli = GetCli();
        _cli.Execute(args);
    }
}