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
        var rootAction = new CalcRootAction();
        var addAction = new CalcAddAction();
        var subAction = new CalcSubAction();
        
        setup.BindAction("calc", rootAction);
        setup.BindAction("calc.add", addAction);
        setup.BindAction("calc.sub", subAction);

        return new ClyshService(setup);
    }
}