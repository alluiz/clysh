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
- 2.1.0 - Remove command interface and protect fields from external manipulation. Builder pattern data handling improvement.
- 2.2.0 - NEW: IgnoreParent action execution flag AND introduce IClyshAction interface

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

**The project of example app is available on [GitHub](https://github.com/alluiz/clysh/tree/master/Samples/Clysh.Sample)**