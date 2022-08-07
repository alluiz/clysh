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

        setup.MakeAction("calc", CliActions.CalcRoot);
        setup.MakeAction("calc.add", CliActions.CalcOperationAdd);
        setup.MakeAction("calc.sub", CliActions.CalcOperationSub);

        return new ClyshService(setup);
    }
}