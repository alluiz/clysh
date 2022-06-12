using Clysh.Core;
using Clysh.Core.Builder;
using Clysh.Data;
using Clysh.Helper;

//var setup = new ClyshSetup("clidata.yml");

void Action1(IClyshCommand _, ClyshMap<ClyshOption> options, IClyshView view)
{
    view.Print(options["test"].Selected ? "mycli with test option" : "mycli without test option");

    if (options["test"].Selected)
    {
        var option = options["test"];

        var data = option.Parameters["value"].Data;

        view.Print(data);
    }
}

//setup.MakeAction("mycli", Action1);

//var cli = new ClyshService(setup, false);
var data = new ClyshData("MyCLI with only test command", "1.0");

var commandBuilder = new ClyshCommandBuilder();
var optionBuilder = new ClyshOptionBuilder();
var parameterBuilder = new ClyshParameterBuilder();

var groupBuilder = new ClyshGroupBuilder();
var fooGroup = groupBuilder.Id("foo").Build();

var myChild = commandBuilder
    .Id("mychild")
    .Description("My child")
    .Build();

var rootCommand = commandBuilder
    .Id("mycli")
    .Description("my own CLI")
    .Group(fooGroup)
    .Option(optionBuilder
        .Id("test", "T")
        .Description("Test option")
        .Parameter(parameterBuilder
            .Id("value")
            .Required(true)
            .Range(1, 15)
            .Build())
        .Group(fooGroup)
        .Build())
    .Option(optionBuilder
        .Id("dummy", "d")
        .Description("Test option")
        .Parameter(parameterBuilder
            .Id("value")
            .Required(true)
            .Range(1, 15)
            .Build())
        .Group(fooGroup)
        .Build())
    .SubCommand(myChild)
    .Action(Action1)
    .Build();

var cli = new ClyshService(rootCommand, new ClyshView(data));

cli.Execute(args);