# CliSharp

## Release Notes

- 1.0.0 - Initial release

## What is?

CliSharp is a library to create a .NET Command Line Interface (CLI). The main goal is to create your own CLI with only business code.

## Features

CliSharp has some features to facilitate the process of create a CLI.

- You can write your own CLI as an YAML or JSON file. Make business changes remotely with no binary update required;
- Easy to make a command tree. You can nest commands to run in a specific order;
- The commands can have custom options with your own shortcuts;
- The options can have required and/or optional parameter to some user data input.

## Getting Started

To use CliSharp you need to install the package from NuGet.

> dotnet add package clisharp

Then, to start create a clidata.yml with the content below:

```
Title: MyCLI
Version: 1.0
CommandsData:
  - Id: mycli
    Description: My own CLI
    OptionsData:
      - Description: Test option
        Id: test
        Shortcut: t
    Root: true
```

To use this create a new Console Application, then in your main method write this:

```
public static void Main(string[] args)
{
    CliSharpDataSetup setup = new("clidata.yml");

    setup.SetCommandAction("mycli", (options, cliFront) =>
    {
        if (options.Has("test"))
            cliFront.PrintWithBreak($"mycli with test option");
        else
            cliFront.PrintWithBreak($"mycli without test option");
    });

    ICliSharpView view = new CliSharpView(new CliSharpConsole(), setup.Data, true);

    ICliSharpService cli = new CliSharpService(setup.RootCommand, view);

    cli.Execute(args);
}
```

Run the console and you will see the magic. If you need some help, pass the argument --help to your app.

> Note: The project of example app is available on ./CliSharp.Example folder