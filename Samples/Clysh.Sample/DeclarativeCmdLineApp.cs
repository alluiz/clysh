using Clysh.Core;

namespace Clysh.Sample;

public class DeclarativeCmdLineApp: ICmdLineApp
{
    public IClyshService Cli { get; }

    public DeclarativeCmdLineApp()
    {
        this.Cli = GetCli();
    }

    private static IClyshService GetCli()
    {
        var setup = new ClyshSetup("clidata.yml");

        setup.Load();

        setup.BindAction("calc", CliActions.CalcRoot);
        setup.BindAction("calc.add", CliActions.CalcOperationAdd);
        setup.BindAction("calc.sub", CliActions.CalcOperationSub);

        return new ClyshService(setup);
    }
}