using System;
using System.IO.Abstractions;
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
        Action<ClyshMap<ClyshOption>,IClyshView> action = (_, _) => {};
        setup.MakeAction("mycli", action);

        var root = setup.RootCommand;
        
        Assert.AreEqual("MyCLI with only test command", setup.Data.Title);
        Assert.AreEqual("1.0", setup.Data.Version);
        
        Assert.AreEqual("mycli", root.Id);
        Assert.AreEqual("My own CLI", root.Description);
        Assert.AreEqual(2, root.Options.Count);
        Assert.AreEqual(1, root.Children.Count);
        Assert.AreEqual(action, root.Action);
        
        Assert.AreEqual("Test option", root.Options["test"].Description);
        Assert.AreEqual("T", root.Options["test"].Shortcut);
        Assert.AreEqual(1, root.Options["test"].Parameters.Count);
        
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

        var root = setup.RootCommand;
        
        Assert.AreEqual("MyCLI with only test command", setup.Data.Title);
        Assert.AreEqual("1.0", setup.Data.Version);
        
        Assert.AreEqual("mycli", root.Id);
        Assert.AreEqual("My own CLI", root.Description);
        Assert.AreEqual(2, root.Options.Count);
        Assert.IsEmpty(root.Children);
        
        Assert.AreEqual("Test option", root.Options["test"].Description);
        Assert.AreEqual("T", root.Options["test"].Shortcut);
        Assert.AreEqual(1, root.Options["test"].Parameters.Count);
        
        Assert.IsTrue(root.Options["test"].Parameters["ab"].Required);
        Assert.AreEqual(1, root.Options["test"].Parameters["ab"].MinLength);
        Assert.AreEqual(15, root.Options["test"].Parameters["ab"].MaxLength);
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
    Children:
      - mychild
  - Id: mychild
    Description: My child";
    }
}