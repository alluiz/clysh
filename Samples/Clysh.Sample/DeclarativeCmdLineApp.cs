using Clysh.Core;
using Microsoft.Extensions.Logging;

namespace Clysh.Sample;

public class DeclarativeCmdLineApp: CmdLineApp
{
    /// <summary>
    /// Get a CLI using declarative YAML (or JSON) file
    /// </summary>
    /// <returns>CLI service</returns>
    protected override IClyshService GetCli()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("Clysh.Core", LogLevel.Debug)
                .AddConsole();
        });
        
        var setup = new ClyshSetup("clidata.yml", loggerFactory.CreateLogger<ClyshSetup>());
        
        var rootAction = new CalcRootAction();
        var addAction = new CalcAddAction();
        var subAction = new CalcSubAction();
        
        setup.BindAction("calc", rootAction);
        setup.BindAction("calc.add", addAction);
        setup.BindAction("calc.sub", subAction);

        return new ClyshService(setup, logger: loggerFactory.CreateLogger<ClyshService>());
    }
}