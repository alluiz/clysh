using Clysh.Core;

namespace Clysh.Sample;

public abstract class CmdLineApp
{
    private IClyshService? _cli;

    protected abstract IClyshService GetCli(); 

    public void Execute(string[] args)
    {
        if (_cli == null) {
            this._cli = GetCli();
        }
        
        this._cli.Execute(args);
    }
}