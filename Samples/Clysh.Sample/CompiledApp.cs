using Clysh.Core;
using Clysh.Core.Builder;
using Clysh.Data;

namespace Clysh.Sample;

public class CompiledApp: IApp
{
    public IClyshService Cli { get; }

    public CompiledApp()
    {
        Cli = GetCli();
    }
    
    private static IClyshService GetCli()
    {
        var data = new ClyshData("MyCalc CLI", "1.0");

        var commandBuilder = new ClyshCommandBuilder();
        var optionBuilder = new ClyshOptionBuilder();
        var parameterBuilder = new ClyshParameterBuilder();
        var groupBuilder = new ClyshGroupBuilder();
        
        var operationGroup = groupBuilder.Id("operation").Build();

        var operationCommand = commandBuilder
            .Id("operation")
            .Description("Select some operation")
            .Group(operationGroup)
            .Option(optionBuilder
                .Id("add")
                .Description("Add two numbers (a + b)")
                .Parameter(parameterBuilder
                    .Id("a")
                    .Required(true)
                    .Range(1, 15)
                    .Order(0)
                    .Build())
                .Parameter(parameterBuilder
                    .Id("b")
                    .Required(true)
                    .Range(1, 15)
                    .Order(1)
                    .Build())
                .Group(operationGroup)
                .Build())
            .Option(optionBuilder
                .Id("sub")
                .Description("Subtract two numbers (a - b)")
                .Parameter(parameterBuilder
                    .Id("a")
                    .Required(true)
                    .Range(1, 15)
                    .Order(0)
                    .Build())
                .Parameter(parameterBuilder
                    .Id("b")
                    .Required(true)
                    .Range(1, 15)
                    .Order(1)
                    .Build())
                .Group(operationGroup)
                .Build())
            .Option(optionBuilder
                .Id("times")
                .Description("Times two numbers (a * b)")
                .Parameter(parameterBuilder
                    .Id("a")
                    .Required(true)
                    .Range(1, 15)
                    .Order(0)
                    .Build())
                .Parameter(parameterBuilder
                    .Id("b")
                    .Required(true)
                    .Range(1, 15)
                    .Order(1)
                    .Build())
                .Group(operationGroup)
                .Build())
            .Option(optionBuilder
                .Id("by")
                .Description("Divide two numbers (a / b)")
                .Parameter(parameterBuilder
                    .Id("a")
                    .Required(true)
                    .Range(1, 15)
                    .Order(0)
                    .Build())
                .Parameter(parameterBuilder
                    .Id("b")
                    .Required(true)
                    .Range(1, 15)
                    .Order(1)
                    .Build())
                .Group(operationGroup)
                .Build())
            .Action(CliActions.Calc)
            .Build();
        
        var rootCommand = commandBuilder
            .Id("calc")
            .Description("My calculator using CLI")
            .RequireSubcommand(true)
            .SubCommand(operationCommand)
            .Build();

        return new ClyshService(rootCommand, new ClyshView(data));
    }
}