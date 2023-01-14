using Clysh.Core;

namespace Clysh.Sample;

public class DeclarativeCmdLineApp: CmdLineApp
{
    protected override IClyshService GetCli()
    {
        var setup = new ClyshSetup("clidata.yml");

        setup.Load();

        setup.BindAction("calc", CliActions.CalcRoot);
        setup.BindAction("calc.add", CliActions.CalcOperationAdd);
        setup.BindAction("calc.sub", CliActions.CalcOperationSub);

        return new ClyshService(setup);
    }
}