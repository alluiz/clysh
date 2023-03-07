using System;
using System.Linq;
using Clysh.Core;
using Clysh.Core.Builder;
using Clysh.Helper;
using NUnit.Framework;

namespace Clysh.Tests;

public class ClyshCommandTests
{
    private ClyshCommandBuilder _commandBuilder = new();

    [SetUp]
    public void Setup()
    {
        _commandBuilder = new ClyshCommandBuilder();
    }

    [Test]
    public void TestShouldThrownErrorWithEmptyId()
    {
        var id = string.Empty;
        var exception = Assert.Throws<EntityException>(() => _commandBuilder.Id(id).Build());
        
        Assert.NotNull(exception);
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateIdLength);
    }
    
    [Test]
    public void TestShouldThrownErrorWithSingleCharId()
    {
        const string id = "t";
        var exception = Assert.Throws<EntityException>(() => _commandBuilder.Id(id).Build());
        
        Assert.NotNull(exception);
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateIdPattern);
    }
    
    [Test]
    public void TestShouldThrownErrorWithSomeSpecialCharId()
    {
        const string id = "te$t";
        var exception = Assert.Throws<EntityException>(() => _commandBuilder.Id(id).Build());
        
        Assert.NotNull(exception);
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateIdPattern);
    }
    
    [Test]
    public void TestShouldThrownErrorWithSomeSpaceIntoId()
    {
        const string id = "test foo";
        var exception = Assert.Throws<EntityException>(() => _commandBuilder.Id(id).Build());
        
        Assert.NotNull(exception);
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateIdPattern);
    }
    
    [Test]
    public void TestShouldThrownErrorWithEmptyDotSequenceId()
    {
        const string id = "test..";
        var exception = Assert.Throws<EntityException>(() => _commandBuilder.Id(id).Build());
        
        Assert.NotNull(exception);
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateIdPattern);
    }
    
    [Test]
    public void TestShouldThrownErrorWithEmptyDotSequenceOnBeginId()
    {
        const string id = "..test";
        var exception = Assert.Throws<EntityException>(() => _commandBuilder.Id(id).Build());
        
        Assert.NotNull(exception);
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateIdPattern);
    }
    
    [Test]
    public void TestShouldThrownErrorWithEmptyDotSequenceBeforeAValidWordId()
    {
        const string id = "test..foo";
        var exception = Assert.Throws<EntityException>(() => _commandBuilder.Id(id).Build());
        
        Assert.NotNull(exception);
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateIdPattern);
    }
    
    [Test]
    public void TestShouldThrownErrorWithTooLongId()
    {
        const string id = "qgheTIcaHKmrDqkpNhwOS26Z9juW0WgO3Gxt20DuyttpjpaFXjvNI0SwtgUiWEr1COR4we2wZsUmehalf6PAXqWPcWI7Nqi14WVYa";
        var exception = Assert.Throws<EntityException>(() => _commandBuilder.Id(id).Build());
        
        Assert.NotNull(exception);
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateIdLength);
    }
    
    [Test]
    public void TestShouldThrownErrorWithoutDescription()
    {
        const string id = "test";
        var exception = Assert.Throws<EntityException>(() => _commandBuilder.Id(id).Build());
        
        Assert.NotNull(exception);
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateDescription);
    }

    [Test]
    public void TestShouldThrownErrorWithShortDescription()
    {
        const string id = "test";
        const string description = "short";
        var exception = Assert.Throws<EntityException>(() => _commandBuilder.Id(id).Description(description).Build());
        
        Assert.NotNull(exception);
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateDescription);
    }
    
    [Test]
    public void TestShouldThrownErrorWithTooLongDescription()
    {
        const string id = "test";
        const string description = "qgheTIcaHKmrDqkpNhwOS26Z9juW0WgO3Gxt20DuyttpjpaFXjvNI0SwtgUiWEr1COR4we2wZsUmehalf6PAXqWPcWI7Nqi14WVYa";
        var exception = Assert.Throws<EntityException>(() => _commandBuilder.Id(id).Description(description).Build());
        
        Assert.NotNull(exception);
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateDescription);
    }
    
    [Test]
    public void TestShouldThrownErrorWithShortDescriptionWithBlankSpacesOnEnd()
    {
        const string id = "test";
        const string description = "short                 ";
        var exception = Assert.Throws<EntityException>(() => _commandBuilder.Id(id).Description(description).Build());
        
        Assert.NotNull(exception);
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateDescription);
    }
    
    [Test]
    public void TestShouldPassWithValidIdAndDescription()
    {
        const string id = "test";
        const string name = "test";
        const string description = "The command test description";
        
        var command = _commandBuilder.Id(id).Description(description).Build();
        
        Assert.NotNull(command);
        Assert.AreEqual(id, command.Id);
        Assert.AreEqual(name, command.Name);
        Assert.AreEqual(description, command.Description);
        Assert.IsFalse(command.Abstract);
    }
    
    [Test]
    public void TestShouldPassWithValidIdWithParentAndDescription()
    {
        const string id = "parent.test";
        const string name = "test";
        const string description = "The command test description";
        
        var command = _commandBuilder.Id(id).Description(description).Build();
        
        Assert.NotNull(command);
        Assert.AreEqual(id, command.Id);
        Assert.AreEqual(name, command.Name);
        Assert.AreEqual(description, command.Description);
    }
    
    [Test]
    public void TestShouldPassWithValidIdWithParentAndNumberAndDescription()
    {
        const string id = "parent.test2";
        const string name = "test2";
        const string description = "The command test description";
        
        var command = _commandBuilder.Id(id).Description(description).Build();
        
        Assert.NotNull(command);
        Assert.AreEqual(id, command.Id);
        Assert.AreEqual(name, command.Name);
        Assert.AreEqual(description, command.Description);
    }
    
    [Test]
    public void TestShouldPassAndRemoveDescriptionBlankspaceFromTheEnd()
    {
        const string id = "parent.test2";
        const string name = "test2";
        const string description = "The command test description      ";
        
        var command = _commandBuilder.Id(id).Description(description).Build();
        
        Assert.NotNull(command);
        Assert.AreEqual(id, command.Id);
        Assert.AreEqual(name, command.Name);
        Assert.AreEqual(description.Trim(), command.Description);
    }
    
    [Test]
    public void TestShouldPassAsAbstractCommandWithSomeSubcommand()
    {
        const string id = "test";
        const string description = "The command test description";
        
        const string subCommandId = "foo";
        const string subCommandDescription = "foo sub command description";
        
        var subCommandBuilder = new ClyshCommandBuilder();

        var subCommand = subCommandBuilder.Id(subCommandId).Description(subCommandDescription).Build();
        
        var command = _commandBuilder.Id(id).Description(description).MarkAsAbstract().SubCommand(subCommand).Build();
        
        Assert.NotNull(command);
        Assert.IsTrue(command.Abstract);
        Assert.IsTrue(command.AnySubcommand());
    }

    [Test]
    public void TestShouldThrownErrorAsAbstractCommandWithoutSubcommand()
    {
        const string id = "test";
        const string description = "The command test description";
        
        var exception = Assert.Throws<EntityException>(() => _commandBuilder.Id(id).Description(description).MarkAsAbstract().Build());
        
        Assert.NotNull(exception);
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateCommandSubcommands);
    }
    
    [Test]
    public void TestShouldPassWithSomeOption()
    {
        const string id = "test";
        const string description = "The command test description";

        var optionBuilder = new ClyshOptionBuilder();

        const string optionId = "option";
        const string optionDescription = "Some option for testing";
        
        var option = optionBuilder.Id(optionId).Description(optionDescription).Build();
        
        var command = _commandBuilder.Id(id).Description(description).Option(option).Build();
        
        Assert.NotNull(command);
        Assert.AreEqual(id, command.Id);
        Assert.AreEqual(description, command.Description);
        Assert.AreEqual(option, command.Options[optionId]);        
        Assert.AreEqual(command, option.Command);
    }
    
    [Test]
    public void TestShouldPassWithSomeOptionIntoGroup()
    {
        const string id = "test";
        const string description = "The command test description";

        var groupBuilder = new ClyshGroupBuilder();
        var optionBuilder = new ClyshOptionBuilder();

        const string optionId = "option";
        const string optionDescription = "Some option for testing";

        const string groupId = "group";

        var group = groupBuilder.Id(groupId).Build();
        
        var option = optionBuilder
            .Id(optionId)
            .Description(optionDescription)
            .Group(group)
            .Build();
        
        var command = _commandBuilder.Id(id).Description(description).Option(option).Build();
        
        Assert.NotNull(command);
        Assert.AreEqual(id, command.Id);
        Assert.AreEqual(description, command.Description);
        Assert.AreEqual(option, command.Options[optionId]);        
        Assert.AreEqual(command, option.Command);
        Assert.AreEqual(group, command.Groups[groupId]);
        Assert.AreEqual(option, command.GetAvailableOptionsFromGroup(groupId).First());
    }
    
    [Test]
    public void TestShouldPassWithSomeOptionWithShortcut()
    {
        const string id = "test";
        const string description = "The command test description";

        var optionBuilder = new ClyshOptionBuilder();

        const string optionId = "option";
        const string optionShortcut = "o";
        const string optionDescription = "Some option for testing";
        
        var option = optionBuilder.Id(optionId, optionShortcut).Description(optionDescription).Build();
        
        var command = _commandBuilder.Id(id).Description(description).Option(option).Build();
        
        Assert.NotNull(command);
        Assert.AreEqual(id, command.Id);
        Assert.AreEqual(description, command.Description);
        Assert.AreEqual(option, command.Options[optionId]);        
        Assert.AreEqual(option, command.GetOption(optionShortcut));        
        Assert.AreEqual(command, option.Command);
    }
    
    [Test]
    public void TestShouldThrownErrorWithSomeOptionWithDuplicatedShortcut()
    {
        const string id = "test";
        const string description = "The command test description";

        var optionBuilder = new ClyshOptionBuilder();

        const string optionDuplicatedId = "other";
        const string optionDuplicatedShortcut = "o";
        const string optionDuplicatedDescription = "Some other option for testing";
        
        const string optionId = "option";
        const string optionShortcut = "o";
        const string optionDescription = "Some option for testing";
        
        var optionDuplicated = optionBuilder.Id(optionDuplicatedId, optionDuplicatedShortcut).Description(optionDuplicatedDescription).Build();
        var option = optionBuilder.Id(optionId, optionShortcut).Description(optionDescription).Build();
        
        var exception = Assert.Throws<EntityException>(() => _commandBuilder
            .Id(id)
            .Description(description)
            .Option(optionDuplicated)
            .Option(option)
            .Build());
        
        ExtendedAssert.MatchMessage(exception?.Message!, "Invalid option shortcut. The command already has an option with shortcut: {0}.");
    }
    
    [Test]
    public void TestShouldThrownErrorWithSomeOptionWithDuplicatedOption()
    {
        const string id = "test";
        const string description = "The command test description";

        var optionBuilder = new ClyshOptionBuilder();

        const string optionDuplicatedId = "option";
        const string optionDuplicatedDescription = "Some other option for testing with the same id";
        
        const string optionId = "option";
        const string optionDescription = "Some option for testing";
        
        var optionDuplicated = optionBuilder.Id(optionDuplicatedId).Description(optionDuplicatedDescription).Build();
        var option = optionBuilder.Id(optionId).Description(optionDescription).Build();
        
        var exception = Assert.Throws<EntityException>(() => _commandBuilder
            .Id(id)
            .Description(description)
            .Option(optionDuplicated)
            .Option(option)
            .Build());
        
        ExtendedAssert.MatchMessage(exception?.Message!, "Invalid option id. The command already has an option with id: {0}.");
    }
    
    [Test]
    public void TestShouldThrownErrorWithSomeOptionAssociatedToAnotherCommand()
    {
        const string id = "test";
        const string description = "The command test description";

        var optionBuilder = new ClyshOptionBuilder();

        const string optionId = "option";
        const string optionShortcut = "o";
        const string optionDescription = "Some option for testing";
        
        var option = optionBuilder.Id(optionId, optionShortcut).Description(optionDescription).Build();

        _commandBuilder
             .Id("other")
             .Description("The other command description")
             .Option(option)
             .Build();

        var exception = Assert.Throws<EntityException>(() => _commandBuilder
            .Id(id)
            .Description(description)
            .Option(option)
            .Build());
        
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateCommandPropertyMemory);
    }
    
    [Test]
    public void TestShouldPassWithSomeGlobalOption()
    {
        const string id = "test";
        const string description = "The command test description";

        var optionBuilder = new ClyshOptionBuilder();

        const string optionId = "option";
        const string optionDescription = "Some option for testing";
        
        var option = optionBuilder.Id(optionId).Description(optionDescription).MarkAsGlobal().Build();
        
        var command = _commandBuilder.Id(id).Description(description).Option(option).Build();
        
        Assert.NotNull(command);
        Assert.AreEqual(id, command.Id);
        Assert.AreEqual(description, command.Description);
        Assert.AreEqual(option, command.Options[optionId]);
        Assert.IsNull(option.Command);
    }
    
    [Test]
    public void TestHelpCommand()
    {
        var command = _commandBuilder
            .Id("test")
            .Description("The test option")
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
            .Description("The child of command")
            .Build();
        
        var command = _commandBuilder
            .Id("aa")
            .Description("The parent of the child")
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
            .Id("test")
            .Description("The test command")
            .Option(optionBuilder
                .Id("option")
                .Description("The option of the command")
                .Build())
            .Build();

        Assert.NotNull(command.Options);
        Assert.AreEqual(4, command.Options.Count);
        Assert.AreEqual("option", command.Options["option"].Id);
    }
}