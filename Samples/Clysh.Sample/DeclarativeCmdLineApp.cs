using Clysh.Core;

namespace Clysh.Sample;

public class DeclarativeCmdLineApp: CmdLineApp
{
    /// <summary>
    /// Get a CLI using declarative YAML (or JSON) file
    /// </summary>
    /// <returns>CLI service</returns>
    protected override IClyshService GetCli()
    {
        var setup = new ClyshSetup("clidata.yml");
        
        setup.BindAction("calc", CliActions.CalcRoot);
        setup.BindAction("calc.add", CliActions.CalcOperationAdd);
        setup.BindAction("calc.sub", CliActions.CalcOperationSub);

        return new ClyshService(setup);
    }
}