using System;
using System.IO.Abstractions;
using Clysh.Core;
using Clysh.Helper;
using Moq;
using NUnit.Framework;

namespace Clysh.Tests;

public class ClyshSetupTests
{
    private readonly Mock<IFileSystem> fs = new();
    private const string Path = "/file";

    [SetUp]
    public void Setup()
    {
        fs.Reset();
    }

    [Test]
    public void CreateSetupYamlSuccessful()
    {
        fs.Setup(x => x.File.Exists(Path)).Returns(true);
        fs.Setup(x => x.Path.HasExtension(Path)).Returns(true);
        fs.Setup(x => x.Path.GetExtension(Path)).Returns(".yaml");
        fs.Setup(x => x.File.ReadAllText(Path)).Returns(GetYamlText());

        var setup = new ClyshSetup(fs.Object, Path);
        Action<IClyshCommand, ClyshMap<ClyshOption>, IClyshView> action = (_, _, _) => { };
        setup.MakeAction("mycli", action);

        var root = setup.RootCommand;

        Assert.IsFalse(setup.IsReadyToProduction());

        Assert.AreEqual("MyCLI with only test command", setup.Data.Title);
        Assert.AreEqual("1.0", setup.Data.Version);

        Assert.AreEqual("mycli", root.Id);
        Assert.AreEqual("My own CLI", root.Description);
        Assert.AreEqual(4, root.Options.Count);
        Assert.AreEqual(1, root.SubCommands.Count);
        Assert.AreEqual(action, root.Action);

        Assert.AreEqual("Test option", root.Options["test"].Description);
        Assert.AreEqual("T", root.Options["test"].Shortcut);
        Assert.AreEqual(1, root.Options["test"].Parameters.Count);
        
        Assert.IsFalse(root.Options["dev"].Selected);
        Assert.IsFalse(root.Options["hom"].Selected);
        Assert.IsTrue(root.Groups.ContainsKey("env"));

        Assert.IsTrue(root.Options["test"].Parameters["ab"].Required);
        Assert.AreEqual(1, root.Options["test"].Parameters["ab"].MinLength);
        Assert.AreEqual(15, root.Options["test"].Parameters["ab"].MaxLength);
    }

    [Test]
    public void CreateSetupJsonSuccessful()
    {
        fs.Setup(x => x.File.Exists(Path)).Returns(true);
        fs.Setup(x => x.Path.HasExtension(Path)).Returns(true);
        fs.Setup(x => x.Path.GetExtension(Path)).Returns(".json");
        fs.Setup(x => x.File.ReadAllText(Path)).Returns(GetJsonText());

        var setup = new ClyshSetup(fs.Object, Path);
        Action<IClyshCommand, ClyshMap<ClyshOption>, IClyshView> action = (_, _, _) => { };
        setup.MakeAction("mycli", action);

        var root = setup.RootCommand;

        Assert.IsTrue(setup.IsReadyToProduction());

        Assert.AreEqual("MyCLI with only test command", setup.Data.Title);
        Assert.AreEqual("1.0", setup.Data.Version);

        Assert.AreEqual("mycli", root.Id);
        Assert.AreEqual("My own CLI", root.Description);
        Assert.AreEqual(2, root.Options.Count);
        Assert.IsEmpty(root.SubCommands);

        Assert.AreEqual("Test option", root.Options["test"].Description);
        Assert.AreEqual("T", root.Options["test"].Shortcut);
        Assert.AreEqual(1, root.Options["test"].Parameters.Count);

        Assert.IsTrue(root.Options["test"].Parameters["ab"].Required);
        Assert.AreEqual(1, root.Options["test"].Parameters["ab"].MinLength);
        Assert.AreEqual(15, root.Options["test"].Parameters["ab"].MaxLength);
    }

    [Test]
    public void CreateSetupYamlDuplicatedCommandIdError()
    {
        fs.Setup(x => x.File.Exists(Path)).Returns(true);
        fs.Setup(x => x.Path.HasExtension(Path)).Returns(true);
        fs.Setup(x => x.Path.GetExtension(Path)).Returns(".yaml");
        fs.Setup(x => x.File.ReadAllText(Path)).Returns(GetYamlCommandIdDuplicatedText());

        var exception = Assert.Throws<ClyshException>(() =>
        {
            var dummy = new ClyshSetup(fs.Object, Path);
        });

        Assert.AreEqual(
            "Invalid commands: The id(s): mycli must be unique check your schema and try again. (Parameter 'commands')",
            exception?.InnerException?.Message);
    }

    [Test]
    public void CreateSetupFileNotExistsError()
    {
        fs.Setup(x => x.File.Exists(Path)).Returns(false);

        var exception = Assert.Throws<ClyshException>(() =>
        {
            var dummy = new ClyshSetup(fs.Object, Path);
        });

        Assert.AreEqual("Invalid path: CLI data file was not found. (Parameter 'path')",
            exception?.InnerException?.Message);
    }

    [Test]
    public void CreateSetupFileExtensionInvalidError()
    {
        fs.Setup(x => x.File.Exists(Path)).Returns(true);
        fs.Setup(x => x.Path.HasExtension(Path)).Returns(true);
        fs.Setup(x => x.Path.GetExtension(Path)).Returns(".txt");

        var exception = Assert.Throws<ClyshException>(() =>
        {
            var dummy = new ClyshSetup(fs.Object, Path);
        });

        Assert.AreEqual(
            "Invalid extension. Only JSON (.json) and YAML (.yml or .yaml) files are supported. (Parameter 'path')",
            exception?.InnerException?.Message);
    }

    [Test]
    public void CreateSetupNoFileExtensionError()
    {
        fs.Setup(x => x.File.Exists(Path)).Returns(true);
        fs.Setup(x => x.Path.HasExtension(Path)).Returns(false);

        var exception = Assert.Throws<ClyshException>(() =>
        {
            var dummy = new ClyshSetup(fs.Object, Path);
        });

        Assert.AreEqual(
            "Invalid extension. Only JSON (.json) and YAML (.yml or .yaml) files are supported. (Parameter 'path')",
            exception?.InnerException?.Message);
    }

    [Test]
    public void CreateSetupYamlWithoutCommandError()
    {
        fs.Setup(x => x.File.Exists(Path)).Returns(true);
        fs.Setup(x => x.Path.HasExtension(Path)).Returns(true);
        fs.Setup(x => x.Path.GetExtension(Path)).Returns(".yaml");
        fs.Setup(x => x.File.ReadAllText(Path)).Returns(GetYamlWithoutCommandText());

        var exception = Assert.Throws<ClyshException>(() =>
        {
            var dummy = new ClyshSetup(fs.Object, Path);
        });

        Assert.AreEqual("Invalid commands: The data must contains at once one command. (Parameter 'Data')",
            exception?.InnerException?.Message);
    }

    [Test]
    public void CreateSetupYamlWithoutRootCommandError()
    {
        fs.Setup(x => x.File.Exists(Path)).Returns(true);
        fs.Setup(x => x.Path.HasExtension(Path)).Returns(true);
        fs.Setup(x => x.Path.GetExtension(Path)).Returns(".yaml");
        fs.Setup(x => x.File.ReadAllText(Path)).Returns(GetYamlWithoutRootCommandText());

        var exception = Assert.Throws<ClyshException>(() =>
        {
            var dummy = new ClyshSetup(fs.Object, Path);
        });

        Assert.AreEqual("Data must have at least one root command. (Parameter 'Data')",
            exception?.InnerException?.Message);
    }

    [Test]
    public void CreateSetupYamlWithRecursiveCommandError()
    {
        fs.Setup(x => x.File.Exists(Path)).Returns(true);
        fs.Setup(x => x.Path.HasExtension(Path)).Returns(true);
        fs.Setup(x => x.Path.GetExtension(Path)).Returns(".yaml");
        fs.Setup(x => x.File.ReadAllText(Path)).Returns(GetYamlWithRecursiveCommandText());

        var exception = Assert.Throws<ClyshException>(() =>
        {
            var dummy = new ClyshSetup(fs.Object, Path);
        });

        Assert.AreEqual("Command Error: The command 'mycli' must not be children of itself: mycli>mycli",
            exception?.InnerException?.Message);
    }
    
    [Test]
    public void CreateSetupYamlWithInvalidChildrenCommandError()
    {
        fs.Setup(x => x.File.Exists(Path)).Returns(true);
        fs.Setup(x => x.Path.HasExtension(Path)).Returns(true);
        fs.Setup(x => x.Path.GetExtension(Path)).Returns(".yaml");
        fs.Setup(x => x.File.ReadAllText(Path)).Returns(GetYamlWithInvalidChildrenCommandText());

        var exception = Assert.Throws<ClyshException>(() =>
        {
            var dummy = new ClyshSetup(fs.Object, Path);
        });

        Assert.AreEqual("Invalid commandId. The id: fake was not found on commands data list.",
            exception?.InnerException?.Message);
    }
    
    [Test]
    public void CreateSetupYamlWithInvalidGroupError()
    {
        fs.Setup(x => x.File.Exists(Path)).Returns(true);
        fs.Setup(x => x.Path.HasExtension(Path)).Returns(true);
        fs.Setup(x => x.Path.GetExtension(Path)).Returns(".yaml");
        fs.Setup(x => x.File.ReadAllText(Path)).Returns(GetYamlWithInvalidGroupText());

        var exception = Assert.Throws<ClyshException>(() =>
        {
            var dummy = new ClyshSetup(fs.Object, Path);
        });

        Assert.AreEqual("Invalid group 'test'. You need to add it to 'Groups' field of command.",
            exception?.InnerException?.Message);
    }

    [Test]
    public void CreateSetupJsonNullError()
    {
        fs.Setup(x => x.File.Exists(Path)).Returns(true);
        fs.Setup(x => x.Path.HasExtension(Path)).Returns(true);
        fs.Setup(x => x.Path.GetExtension(Path)).Returns(".json");
        fs.Setup(x => x.File.ReadAllText(Path)).Returns("null");

        var ex = Assert.Throws<ClyshException>(() =>
        {
            var dummy = new ClyshSetup(fs.Object, Path);
        });

        Assert.AreEqual("Invalid JSON: The deserialization results in null object.", ex?.InnerException?.Message);
    }
    
    private string GetYamlWithInvalidGroupText()
    {
        return @"
Title: MyCLI with only test command
Version: 1.0
Commands:
  - Id: mycli
    Description: My own CLI
    Options:
      - Description: Test option
        Id: test
        Shortcut: T
        Group: test
    Root: true
    SubCommands:
      - mychild
  - Id: mychild
    Description: My child";
    }

    private string GetYamlWithInvalidChildrenCommandText()
    {
        return @"
Title: MyCLI with only test command
Version: 1.0
Commands:
  - Id: mycli
    Description: My own CLI
    Options:
      - Description: Test option
        Id: test
        Shortcut: T
        Parameters:
          - Id: ab
            Required: true
            MinLength: 1
            MaxLength: 15
    Root: true
    SubCommands:
      - fake";
    }

    private string GetYamlWithRecursiveCommandText()
    {
        return @"
Title: MyCLI with only test command
Version: 1.0
Commands:
  - Id: mycli
    Description: My own CLI
    Options:
      - Description: Test option
        Id: test
        Shortcut: T
        Parameters:
          - Id: ab
            Required: true
            MinLength: 1
            MaxLength: 15
    Root: true
    SubCommands:
      - mycli";
    }

    private string GetYamlCommandIdDuplicatedText()
    {
        return @"
Title: MyCLI with only test command
Version: 1.0
Commands:
  - Id: mycli
    Description: My own CLI
    Options:
      - Description: Test option
        Id: test
        Shortcut: T
        Parameters:
          - Id: ab
            Required: true
            MinLength: 1
            MaxLength: 15
    Root: true
  - Id: mycli
    Description: My child";
    }

    private string GetJsonText()
    {
        return @"
{
  ""Title"": ""MyCLI with only test command"",
  ""Version"": 1.0,
  ""Commands"": [
    {
      ""Id"": ""mycli"",
      ""Description"": ""My own CLI"",
      ""Options"": [
        {
          ""Description"": ""Test option"",
          ""Id"": ""test"",
          ""Shortcut"": ""T"",
          ""Parameters"": [
            {
              ""Id"": ""ab"",
              ""Required"": true,
              ""MinLength"": 1,
              ""MaxLength"": 15
            }
          ]
        }
      ],
      ""Root"": true
    }
  ]
}";
    }

    private string GetYamlText()
    {
        return @"
Title: MyCLI with only test command
Version: 1.0
Commands:
  - Id: mycli
    Description: My own CLI
    Groups:
      - env
    Options:
      - Description: Test option
        Id: dev
        Group: env
      - Description: Test option
        Id: hom
        Group: env
      - Description: Test option
        Id: test
        Shortcut: T
        Parameters:
          - Id: ab
            Required: true
            MinLength: 1
            MaxLength: 15
    Root: true
    SubCommands:
      - mychild
  - Id: mychild
    Description: My child";
    }

    private string GetYamlWithoutRootCommandText()
    {
        return @"
Title: MyCLI with only test command
Version: 1.0
Commands:
  - Id: mycli
    Description: My own CLI
    Options:
      - Description: Test option
        Id: test
        Shortcut: T
        Parameters:
          - Id: ab
            Required: true
            MinLength: 1
            MaxLength: 15
    SubCommands:
      - mychild
  - Id: mychild
    Description: My child";
    }

    private string GetYamlWithoutCommandText()
    {
        return @"
Title: MyCLI with only test command
Version: 1.0";
    }
}