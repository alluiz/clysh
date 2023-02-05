using Clysh.Core;
using Clysh.Core.Builder;
using NUnit.Framework;

namespace Clysh.Tests;

public class ClyshCommandTests
{
    private readonly ClyshCommandBuilder _commandBuilder = new();
    
    [Test]
    public void TestHelpCommand()
    {
        var command = _commandBuilder
            .Id("aa")
            .Build();
        
        var help = command.Options["help"];
        Assert.NotNull(help);
        Assert.AreEqual("help", help.Id);
        Assert.AreEqual("h", help.Shortcut);
        Assert.AreEqual("Show help on screen", help.Description);
        Assert.IsEmpty(help.Parameters);
    }

    [Test]
    public void TestAddChild()
    {
        var child = _commandBuilder
            .Id("child")
            .Build();
        
        var command = _commandBuilder
            .Id("aa")
            .SubCommand(child)
            .Build();
        
        Assert.NotNull(command.SubCommands);
        Assert.AreEqual(1, command.SubCommands.Count);
        Assert.NotNull(command.SubCommands["child"]);
    }

    [Test]
    public void TestAddOption()
    {
        var optionBuilder = new ClyshOptionBuilder();
        
        var command = _commandBuilder
            .Id("aa")
            .Option(optionBuilder
                .Id("option")
                .Build())
            .Build();
        
        Assert.NotNull(command.Options);
        Assert.AreEqual(4, command.Options.Count);
        Assert.AreEqual("option", command.Options["option"].Id);
    }
}