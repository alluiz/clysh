using Clysh.Core;

var setup = new ClyshSetup("clidata.yml");

setup.MakeAction("mycli", (_, options, view) =>
{
    view.Print(options["test"].Selected ? "mycli with test option" : "mycli without test option");

    if (options["test"].Selected)
    {
        var option = options["test"];

        var data = option.Parameters["value"].Data;

        view.Print(data);
    }
});

var cli = new ClyshService(setup, false);

cli.Execute(args);