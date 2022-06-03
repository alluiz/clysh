using NUnit.Framework;

namespace Clysh.Tests;

public class ClyshCommandTests
{
    private ClyshCommand command = new();

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
        ClyshCommandBuilder builder = new ClyshCommandBuilder();
        var child = builder.Id("child").Build();
        command.AddChild(child);
        Assert.NotNull(command.Children);
        Assert.AreEqual(1, command.Children.Count);
        Assert.NotNull(command.Children["child"]);
    }

    [Test]
    public void TestAddOption()
    {
        ClyshOptionBuilder builder = new ClyshOptionBuilder();
        command.AddOption(builder
            .Id("option")
            .Build());
        Assert.NotNull(command.AvailableOptions);
        Assert.AreEqual(2, command.AvailableOptions.Count);
        Assert.AreEqual("option", command.AvailableOptions["option"].Id);
    }
}