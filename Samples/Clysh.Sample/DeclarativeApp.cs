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

        setup.MakeAction("operation", CliActions.Calc);

        return new ClyshService(setup);
    }
}