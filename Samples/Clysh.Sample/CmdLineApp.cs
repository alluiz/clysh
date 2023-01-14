using Clysh.Core;

namespace Clysh.Sample;

public abstract class CmdLineApp
{
    protected IClyshService cli;

    protected CmdLineApp()
    {
        this.cli = GetCli();
    }

    protected abstract IClyshService GetCli(); 

    public void Execute(string[] args)
    {
        this.cli.Execute(args);
    }
}