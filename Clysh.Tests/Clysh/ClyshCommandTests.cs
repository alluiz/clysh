using Clysh.Core;
using Clysh.Core.Builder;
using NUnit.Framework;

namespace Clysh.Tests;

public class ClyshCommandTests
{
    private ClyshCommand command = default!;

    [SetUp]
    public void Setup()
    {
        command = new ClyshCommand();
    }

    [Test]
    public void TestHelpCommand()
    {
        var help = command.GetOption("help");
        Assert.NotNull(help);
        Assert.AreEqual("help", help.Id);
        Assert.AreEqual("h", help.Shortcut);
        Assert.AreEqual("Show help on screen", help.Description);
        Assert.IsEmpty(help.Parameters);
    }

    [Test]
    public void TestAddChild()
    {
        var builder = new ClyshCommandBuilder();
        var child = builder.Id("child").Build();
        command.AddSubCommand(child);
        Assert.NotNull(command.SubCommands);
        Assert.AreEqual(1, command.SubCommands.Count);
        Assert.NotNull(command.SubCommands["child"]);
    }

    [Test]
    public void TestAddOption()
    {
        var builder = new ClyshOptionBuilder();
        command.AddOption(builder
            .Id("option")
            .Build());
        Assert.NotNull(command.Options);
        Assert.AreEqual(4, command.Options.Count);
        Assert.AreEqual("option", command.Options["option"].Id);
    }
}