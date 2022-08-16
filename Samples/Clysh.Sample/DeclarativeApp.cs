using Clysh.Core;

namespace Clysh.Sample;

public class DeclarativeApp: IApp
{
    public IClyshService Cli { get; }

    public DeclarativeApp()
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