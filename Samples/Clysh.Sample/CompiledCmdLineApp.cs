using Clysh.Core;
using Clysh.Core.Builder;
using Clysh.Data;
using Microsoft.Extensions.Logging;

namespace Clysh.Sample;

public class CompiledCmdLineApp: CmdLineApp
{
    protected override IClyshService GetCli()
    {
        var data = new ClyshData("MyCalc CLI", "1.0");

        var commandBuilder = new ClyshCommandBuilder();
        var optionBuilder = new ClyshOptionBuilder();
        var parameterBuilder = new ClyshParameterBuilder();
        
        var addOperationCommand = commandBuilder
            .Id("calc.add")
            .Description("Add some values")
            .Option(optionBuilder
                .Id("values")
                .Description("Add two numbers (a + b)")
                .Parameter(parameterBuilder
                    .Id("a")
                    .MarkAsRequired()
                    .Range(1, 15)
                    .Order(0)
                    .Build())
                .Parameter(parameterBuilder
                    .Id("b")
                    .MarkAsRequired()
                    .Range(1, 15)
                    .Order(1)
                    .Build())
                .Build())
            .Action(new CalcAddAction())
            .Build();
        
        var subOperationCommand = commandBuilder
            .Id("calc.sub")
            .Description("Subtract some values")
            .Option(optionBuilder
                .Id("values")
                .Description("Subtract two numbers (a - b)")
                .Parameter(parameterBuilder
                    .Id("a")
                    .MarkAsRequired()
                    .Range(1, 15)
                    .Order(0)
                    .Build())
                .Parameter(parameterBuilder
                    .Id("b")
                    .MarkAsRequired()
                    .Range(1, 15)
                    .Order(1)
                    .Build())
                .Build())
            .Action(new CalcSubAction())
            .Build();
        
        var rootCommand = commandBuilder
            .Id("calc")
            .Description("My calculator using CLI")
            .MarkAsAbstract()
            .SubCommand(addOperationCommand)
            .SubCommand(subOperationCommand)
            .Build();

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("Clysh.Core", LogLevel.Debug)
                .AddConsole();
        });
        
        return new ClyshService(rootCommand, new ClyshView(data), logger: loggerFactory.CreateLogger<ClyshService>());
    }
}