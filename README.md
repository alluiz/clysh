# Clysh

**CL**i in **Y**aml for **SH**ell apps

Create your **own CLI on .NET 6+** with simple steps.

## Release Notes

- 1.0.2 - Initial release
- 1.0.3 - Parameters creation using builder pattern

## What is?

**Clysh** is a library to create a .NET _Command Line Interface_ (CLI). The main goal is to create your own CLI with **only business code**.

## Features

Clysh has some features to **facilitate** the process of create a CLI.

- You can write your own CLI as an **YAML** or **JSON** file. The file is parsed at runtime.
- Easy to **make a command tree**. You can nest commands to run in a specific order;
- The commands can have **custom options** with your own shortcuts;
- The options can have **required and/or optional parameter** to some user data input.
- You can group options like **a radio button**.

## Getting Started

To use Clysh you need to install the package from NuGet.

> dotnet add package clysh

Then, to start create a _clidata.yml_ with the content below:

```
Title: MyCLI with only test command
Version: 1.0
Commands:
  - Id: mycli
    Description: My own CLI
    Groups:
      - foo
    Options:
      - Description: Test option
        Id: test
        Shortcut: T
        Parameters:
          - Id: value
            Required: true
            MinLength: 1
            MaxLength: 15
        Group: foo
      - Description: Test option
        Id: dummy
        Shortcut: d
        Parameters:
          - Id: value
            Required: true
            MinLength: 1
            MaxLength: 15
        Group: foo
    Root: true
    SubCommands:
      - mychild
  - Id: mychild
    Description: My child
```

To use this create a new **Console Application**, then in your **Program.cs** write this:

```
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

var cli = new ClyshService(setup, true);

cli.Execute(args);
```

Run the console and you will see the **magic**. If you need some help, pass the argument **--help** to your app.

The expected output:

> mycli without test option

With **--help** argument

```
MyCLI with only test command. Version: 1.0

Usage: mycli [options] [commands]

My own CLI

[options]:

   Shortcut   Option       Group          Description                                            Parameters: (R)equired | (O)ptional = Length

  -h        --help                        Show help on screen
  -d        --dummy        foo            Test option                                            [0:<value:R>]: 1
  -T        --test         foo            Test option                                            [0:<value:R>]: 1

[commands]:

   mychild                                My child  
```

**Note: The project of example app is available on ./Samples/Clysh.Sample folder**