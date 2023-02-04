using Clysh.Core;
using Clysh.Core.Builder;
using NUnit.Framework;

namespace Clysh.Tests;

public class ClyshCommandTests
{
    private ClyshCommand _command = default!;

    [SetUp]
    public void Setup()
    {
        _command = new ClyshCommand();
    }

    [Test]
    public void TestHelpCommand()
    {
        var help = _command.GetOption("help");
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
        _command.AddSubCommand(child);
        Assert.NotNull(_command.SubCommands);
        Assert.AreEqual(1, _command.SubCommands.Count);
        Assert.NotNull(_command.SubCommands["child"]);
    }

    [Test]
    public void TestAddOption()
    {
        var builder = new ClyshOptionBuilder();
        _command.AddOption(builder
            .Id("option")
            .Build());
        Assert.NotNull(_command.Options);
        Assert.AreEqual(4, _command.Options.Count);
        Assert.AreEqual("option", _command.Options["option"].Id);
    }
}