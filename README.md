# Clysh

**CL**i in **Y**aml for **SH**ell apps

Create your **own CLI on .NET 6+** with simple steps.

## Release Notes

- 1.0.0 - Initial release

## What is?

**Clysh** is a library to create a .NET _Command Line Interface_ (CLI). The main goal is to create your own CLI with **only business code**.

## Features

Clysh has some features to **facilitate** the process of create a CLI.

- You can write your own CLI as an **YAML** or **JSON** file. Make business changes remotely with no binary update required;
- Easy to **make a command tree**. You can nest commands to run in a specific order;
- The commands can have **custom options** with your own shortcuts;
- The options can have **required and/or optional parameter** to some user data input.

## Getting Started

To use Clysh you need to install the package from NuGet.

> dotnet add package clysh

Then, to start create a _clidata.yml_ with the content below:

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

To use this create a new **Console Application**, then in your main method write this:

```
public static void Main(string[] args)
{
    ClyshDataSetup setup = new("clidata.yml");

    setup.SetCommandAction("mycli", (options, cliFront) =>
    {
        if (options.Has("test"))
            cliFront.PrintWithBreak($"mycli with test option");
        else
            cliFront.PrintWithBreak($"mycli without test option");
    });
    
    IClyshService cli = new ClyshService(setup.RootCommand, new ClyshConsole(), setup.Data);

    cli.Execute(args);
}
```

Run the console and you will see the **magic**. If you need some help, pass the argument **--help** to your app.

The expected output:

> mycli without test option

With --help argument

```
MyCLI

Usage: mycli [options]

My own CLI

[options]:

   Abbrev.    Option                      Description                                            Parameters: (R)equired | (O)ptional = Length

            --help                        Show help on screen                                    
  -t        --test                        Test option      
```

**Note: The project of example app is available on ./Clysh.Example folder**