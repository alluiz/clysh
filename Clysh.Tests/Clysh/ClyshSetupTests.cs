using System;
using System.IO.Abstractions;
using Clysh.Core;
using Clysh.Helper;
using Moq;
using NUnit.Framework;

namespace Clysh.Tests;

public class ClyshSetupTests
{
    private const string Path = "/file";
    private readonly Mock<IFileSystem> fs = new();

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

        var setup = new ClyshSetup(Path, fs.Object);
        
        setup.BindAction("mycli", EmptyAction);

        var root = setup.RootCommand;

        Assert.AreEqual("MyCLI with only test command", setup.Data.Title);
        Assert.AreEqual("1.0", setup.Data.Version);

        Assert.AreEqual("mycli", root.Id);
        Assert.AreEqual("My own CLI", root.Description);
        Assert.AreEqual(6, root.Options.Count);
        Assert.AreEqual(1, root.SubCommands.Count);
        Assert.AreEqual(EmptyAction, root.Action);
        Assert.IsFalse(root.RequireSubcommand);

        Assert.AreEqual("Test option", root.Options["test"].Description);
        Assert.AreEqual("T", root.Options["test"].Shortcut);
        Assert.AreEqual(1, root.Options["test"].Parameters.Count);

        Assert.IsFalse(root.Options["dev"].Selected);
        Assert.IsFalse(root.Options["hom"].Selected);
        Assert.IsTrue(root.Groups.ContainsKey("env"));

        Assert.IsTrue(root.Options["test"].Parameters["ab"].Required);
        Assert.AreEqual(1, root.Options["test"].Parameters["ab"].MinLength);
        Assert.AreEqual(15, root.Options["test"].Parameters["ab"].MaxLength);
        Assert.AreEqual(@"\w+", root.Options["test"].Parameters["ab"].PatternData);
    }

    [Test]
    public void CreateSetupYmlSuccessful()
    {
        fs.Setup(x => x.File.Exists(Path)).Returns(true);
        fs.Setup(x => x.Path.HasExtension(Path)).Returns(true);
        fs.Setup(x => x.Path.GetExtension(Path)).Returns(".yml");
        fs.Setup(x => x.File.ReadAllText(Path)).Returns(GetYamlText());

        var setup = new ClyshSetup(Path, fs.Object);
        
        setup.BindAction("mycli", EmptyAction);

        var root = setup.RootCommand;

        Assert.AreEqual("MyCLI with only test command", setup.Data.Title);
        Assert.AreEqual("1.0", setup.Data.Version);

        Assert.AreEqual("mycli", root.Id);
        Assert.AreEqual("My own CLI", root.Description);
        Assert.AreEqual(6, root.Options.Count);
        Assert.AreEqual(1, root.SubCommands.Count);
        Assert.AreEqual(EmptyAction, root.Action);
        Assert.IsFalse(root.RequireSubcommand);

        Assert.AreEqual("Test option", root.Options["test"].Description);
        Assert.AreEqual("T", root.Options["test"].Shortcut);
        Assert.AreEqual(1, root.Options["test"].Parameters.Count);

        Assert.IsFalse(root.Options["dev"].Selected);
        Assert.IsFalse(root.Options["hom"].Selected);
        Assert.IsTrue(root.Groups.ContainsKey("env"));

        Assert.IsTrue(root.Options["test"].Parameters["ab"].Required);
        Assert.AreEqual(1, root.Options["test"].Parameters["ab"].MinLength);
        Assert.AreEqual(15, root.Options["test"].Parameters["ab"].MaxLength);
        Assert.AreEqual(@"\w+", root.Options["test"].Parameters["ab"].PatternData);
    }

    [Test]
    public void CreateSetupJsonSuccessful()
    {
        fs.Setup(x => x.File.Exists(Path)).Returns(true);
        fs.Setup(x => x.Path.HasExtension(Path)).Returns(true);
        fs.Setup(x => x.Path.GetExtension(Path)).Returns(".json");
        fs.Setup(x => x.File.ReadAllText(Path)).Returns(GetJsonText());

        var setup = new ClyshSetup(Path, fs.Object);
        

        setup.BindAction("mycli", EmptyAction);

        var root = setup.RootCommand;

        Assert.AreEqual("MyCLI with only test command", setup.Data.Title);
        Assert.AreEqual("1.0", setup.Data.Version);

        Assert.AreEqual("mycli", root.Id);
        Assert.AreEqual("My own CLI", root.Description);
        Assert.AreEqual(4, root.Options.Count);
        Assert.IsEmpty(root.SubCommands);

        Assert.AreEqual("Test option", root.Options["test"].Description);
        Assert.AreEqual("T", root.Options["test"].Shortcut);
        Assert.AreEqual(1, root.Options["test"].Parameters.Count);

        Assert.IsTrue(root.Options["test"].Parameters["ab"].Required);
        Assert.AreEqual(1, root.Options["test"].Parameters["ab"].MinLength);
        Assert.AreEqual(15, root.Options["test"].Parameters["ab"].MaxLength);
    }

    private void EmptyAction(IClyshCommand clyshCommand, IClyshView clyshView)
    {
        //Just a empty reference to test address memory
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
            var setup = new ClyshSetup(Path, fs.Object);
            
        });

        ExtendedAssert.MatchMessage(exception?.InnerException?.Message!, ClyshMessages.MessageInvalidCommandsDuplicated);
    }

    [Test]
    public void CreateSetupFileNotExistsError()
    {
        fs.Setup(x => x.File.Exists(Path)).Returns(false);

        var exception = Assert.Throws<ClyshException>(() =>
        {
            var setup = new ClyshSetup(Path, fs.Object);
            
        });

        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.MessageInvalidFilePath);
    }

    [Test]
    public void CreateSetupFileExtensionInvalidError()
    {
        fs.Setup(x => x.File.Exists(Path)).Returns(true);
        fs.Setup(x => x.Path.HasExtension(Path)).Returns(true);
        fs.Setup(x => x.Path.GetExtension(Path)).Returns(".txt");

        var exception = Assert.Throws<ClyshException>(() =>
        {
            var setup = new ClyshSetup(Path, fs.Object);
            
        });

        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.MessageInvalidFileExtension);
    }

    [Test]
    public void CreateSetupNoFileExtensionError()
    {
        fs.Setup(x => x.File.Exists(Path)).Returns(true);
        fs.Setup(x => x.Path.HasExtension(Path)).Returns(false);

        var exception = Assert.Throws<ClyshException>(() =>
        {
            var setup = new ClyshSetup(Path, fs.Object);
            
        });

        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.MessageInvalidFileExtension);
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
            var setup = new ClyshSetup(Path, fs.Object);
            
        });

        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.MessageInvalidCommandsLengthAtLeastOne);
    }

    [Test]
    public void CreateSetupYamlWithoutSubCommandError()
    {
        fs.Setup(x => x.File.Exists(Path)).Returns(true);
        fs.Setup(x => x.Path.HasExtension(Path)).Returns(true);
        fs.Setup(x => x.Path.GetExtension(Path)).Returns(".yaml");
        fs.Setup(x => x.File.ReadAllText(Path)).Returns(GetYamlWithoutSubCommandText());

        var exception = Assert.Throws<ClyshException>(() =>
        {
            var setup = new ClyshSetup(Path, fs.Object);
            
        });

        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnCreateCommand);
        ExtendedAssert.MatchMessage(exception?.InnerException?.Message!, ClyshMessages.RequiredSubCommand);
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
            var setup = new ClyshSetup(Path, fs.Object);
            
        });

        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.MessageInvalidCommandsDuplicatedRoot);
    }
    
    [Test]
    public void CreateSetupYamlWithMoreThanOneRootCommandError()
    {
        fs.Setup(x => x.File.Exists(Path)).Returns(true);
        fs.Setup(x => x.Path.HasExtension(Path)).Returns(true);
        fs.Setup(x => x.Path.GetExtension(Path)).Returns(".yaml");
        fs.Setup(x => x.File.ReadAllText(Path)).Returns(GetYamlWithMoreThanOneRootCommandText());

        var exception = Assert.Throws<ClyshException>(() =>
        {
            var setup = new ClyshSetup(Path, fs.Object);
            
        });

        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.MessageInvalidCommandsDuplicatedRoot);
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
            var setup = new ClyshSetup(Path, fs.Object);
            
        });
        
        ExtendedAssert.MatchMessage(exception?.Message, ClyshMessages.ErrorOnCreateCommand);
        ExtendedAssert.MatchMessage(exception?.InnerException?.Message, ClyshMessages.ErrorOnCreateSubCommand);
        ExtendedAssert.MatchMessage(exception?.InnerException?.InnerException?.Message!, ClyshMessages.InvalidCommandDuplicatedWords);
    }

    [Test]
    public void CreateSetupYamlWithDuplicatedParameterOrderError()
    {
        fs.Setup(x => x.File.Exists(Path)).Returns(true);
        fs.Setup(x => x.Path.HasExtension(Path)).Returns(true);
        fs.Setup(x => x.Path.GetExtension(Path)).Returns(".yaml");
        fs.Setup(x => x.File.ReadAllText(Path)).Returns(GetYamlWithDuplicatedParameterOrderText());

        var exception = Assert.Throws<ClyshException>(() =>
        {
            var setup = new ClyshSetup(Path, fs.Object);
            
        });

        ExtendedAssert.MatchMessage(exception?.InnerException?.InnerException?.Message!, ClyshMessages.InvalidParameterOrder);
    }

    [Test]
    public void CreateSetupYamlWithParameterRequiredOrderError()
    {
        fs.Setup(x => x.File.Exists(Path)).Returns(true);
        fs.Setup(x => x.Path.HasExtension(Path)).Returns(true);
        fs.Setup(x => x.Path.GetExtension(Path)).Returns(".yaml");
        fs.Setup(x => x.File.ReadAllText(Path)).Returns(GetYamlWithParameterRequiredOrderText());

        var exception = Assert.Throws<ClyshException>(() =>
        {
            var setup = new ClyshSetup(Path, fs.Object);
            
        });
        
        ExtendedAssert.MatchMessage(exception?.Message, ClyshMessages.ErrorOnCreateCommand);
        ExtendedAssert.MatchMessage(exception?.InnerException?.Message, ClyshMessages.ErrorOnCreateOption);
        ExtendedAssert.MatchMessage(exception?.InnerException?.InnerException?.Message!, ClyshMessages.InvalidParameterRequiredOrder);
    }

    [Test]
    public void CreateSetupYamlWithInvalidParentCommandError()
    {
        fs.Setup(x => x.File.Exists(Path)).Returns(true);
        fs.Setup(x => x.Path.HasExtension(Path)).Returns(true);
        fs.Setup(x => x.Path.GetExtension(Path)).Returns(".yaml");
        fs.Setup(x => x.File.ReadAllText(Path)).Returns(GetYamlWithInvalidParentCommandText());

        var exception = Assert.Throws<ClyshException>(() =>
        {
            var setup = new ClyshSetup(Path, fs.Object);
            
        });

        ExtendedAssert.MatchMessage(exception?.Message, ClyshMessages.MessageInvalidCommandsParent);
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
            var setup = new ClyshSetup(Path, fs.Object);
            
        });

        Assert.AreEqual("The given key 'test' was not present in the dictionary.",
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
            var setup = new ClyshSetup(Path, fs.Object);
            
        });

        ExtendedAssert.MatchMessage(ex?.Message, ClyshMessages.MessageInvalidFileJson);
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
  - Id: mycli.mychild
    Description: My child";
    }

    private string GetYamlWithInvalidParentCommandText()
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
            Order: 1
    Root: true
  - Id: mytest.test
    Description: My test";
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
            Order: 1
    Root: true
  - Id: mycli.mycli
    Description: My own CLI";
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
              ""MaxLength"": 15,
              ""Order"": 1
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
            Pattern: \w+
            Order: 1
    Root: true
  - Id: mycli.mychild
    Description: My awesome child
    RequireSubcommand: false";
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
  - Id: mycli.mychild
    Description: My child";
    }
    
    private string GetYamlWithMoreThanOneRootCommandText()
    {
        return @"
Title: MyCLI with only test command
Version: 1.0
Commands:
  - Id: mycli
    Description: My own CLI
    Root: true
    Options:
      - Description: Test option
        Id: test
        Shortcut: T
        Parameters:
          - Id: ab
            Required: true
            MinLength: 1
            MaxLength: 15
  - Id: mycli.mychild
    Description: My child
    Root: true";
    }

    private string GetYamlWithoutCommandText()
    {
        return @"
Title: MyCLI with only test command
Version: 1.0";
    }

    private string GetYamlWithoutSubCommandText()
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
            Pattern: \w+
            Order: 1
    Root: true
    RequireSubcommand: true";
    }

    private string GetYamlWithDuplicatedParameterOrderText()
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
            Pattern: \w+
            Order: 0
          - Id: c
            Required: true
            MinLength: 1
            MaxLength: 15
            Pattern: \w+
            Order: 0
    Root: true";
    }

    private string GetYamlWithParameterRequiredOrderText()
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
            Required: false
            MinLength: 1
            MaxLength: 15
            Pattern: \w+
            Order: 1
          - Id: c
            Required: true
            MinLength: 1
            MaxLength: 15
            Pattern: \w+
            Order: 2
    Root: true";
    }
}