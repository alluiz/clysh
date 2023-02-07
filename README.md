# Clysh

**CL**i in **Y**aml for **SH**ell apps

Create your **own CLI on .NET 6+** with simple steps.

## Release Notes

- 1.0.2 - Initial release
- 1.0.3 - Parameters creation using builder pattern
- 1.0.4 - XML docs for all public classes
- 1.0.5 - More production safety validations
- 1.0.6 - Simplify option check id
- 1.0.7 - Order parameter by property and introduce filled property to parameter model
- 1.0.8 - BugFix: Lastcommand executed flag fix
- 1.0.9 - Some improvements to order parameters and increase max description to 100 chars
- 1.1.0 - BugFix: Parameter filled order
- 1.2.0 - Make action by command path
- 1.3.1 - BugFix: Debug mode
- 1.3.4 - Data transfer between comands and description breakline after each 50 chars
- 1.3.5 - Improvements to exception handling
- 1.3.6 - Custom error messages and Colorized Console messages
- 1.4.0 - Removed option parameter from action interface
- 1.4.1 - Some code refactoring and fix example
- 2.0.0 - New Feature: Global Options
- 2.0.1 - BugFix: Ignore 'Groups' deprecated field on YAML deserialize
- 3.0.0-beta - Remove command interface and protect fields from external manipulation. Builder pattern data handling improvement.

## What is?

**Clysh** is a library to create a .NET _Command Line Interface_ (CLI). The main goal is to create your own CLI with **only business code**.

## Features

Clysh has some features to **facilitate** the process of create a CLI.

- You can write your own CLI as an **YAML** or **JSON** file. The file is parsed at runtime.
- Easy to **make a command tree**. You can nest commands to run in a specific order;
- The commands can have **custom options** with your own shortcuts;
- The options can have **required and/or optional parameter** to some user data input.
- You can group options like **a radio button**.
- The **help** command is auto-generated.
- Easy to mocking for unit tests.
- 2.0 NEW: Global options feature to reuse option structure with N commands

## Getting Started

To use Clysh you need to install the package from NuGet.

> dotnet add package clysh

Then, to start create a _clidata.yml_ with the content below:

``` yaml
Title: MyCLI with only test command
Version: 1.0
Commands:
  - Id: mycli
    Description: My own CLI
    Options:
      - Id: test
        Description: Test option
        Shortcut: T
        Parameters:
          - Id: value
            Required: true
            MinLength: 1
            MaxLength: 15
        Group: foo
      - Id: dummy
        Description: Dummy option      
        Shortcut: d
        Parameters:
          - Id: value
            Required: true
            MinLength: 1
            MaxLength: 15
        Group: foo
    Root: true
  - Id: mycli.mychild
    Description: My child for tests
```

To use this create a new **Console Application**, then in your **Program.cs** write this:

``` csharp
using Clysh.Core;

var setup = new ClyshSetup("clidata.yml");

setup.BindAction("mycli", (cmd, view) =>
{
    view.Print(cmd.Options["test"].Selected ? "mycli with test option" : "mycli without test option");

    if (cmd.Options["test"].Selected)
    {
        var option = cmd.Options["test"];

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

   Option       Group          Description             Parameters

   -h, --help                  Show help on screen
   -d, --dummy  foo            Test option             <value:Required>
   -T, --test   foo            Test option             <value:Required>

[commands]:

   mychild                                My child  
```

**Note: The project of example app is available on ./Samples/Clysh.Sample folder**