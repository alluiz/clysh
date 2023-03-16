using Clysh.Core;
using Microsoft.Extensions.Logging;

namespace Clysh.Sample;

public class DeclarativeCmdLineApp: CmdLineApp
{
    /// <summary>
    /// Get a CLI using declarative YAML (or JSON) file
    /// </summary>
    /// <returns>CLI service</returns>
    protected override IQuickClysh GetCli()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("Clysh.Core", LogLevel.Debug)
                .AddConsole();
        });
        
        var cli = new QuickClysh("clidata.yml", loggerFactory);

        cli.BindAction("calc", new CalcRootAction());
        cli.BindAction("calc.add", new CalcAddAction());
        cli.BindAction("calc.sub", new CalcSubAction());

        return cli;
    }
}