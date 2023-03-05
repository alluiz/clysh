# Clysh

**CL**i in **Y**aml for **SH**ell apps

Create your **own CLI on .NET 6+** with simple steps.

## Release Notes

- 2.2.0 - NEW: IgnoreParent action execution flag AND introduce IClyshAction interface
- 2.3.0 - NEW: Introduce IClyshCommand interface for unit tests again

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