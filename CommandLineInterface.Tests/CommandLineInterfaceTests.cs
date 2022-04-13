using System;
using System.Collections.Generic;
using CommandLineInterface;
using Moq;
using NUnit.Framework;

namespace CommandLineInterface.Tests;

public class Tests
{

    private Metadata metadata = new Metadata("TestCLI", "test", "test command for unit tests");
    private Mock<IConsoleManager> consoleMock = new Mock<IConsoleManager>();

    [SetUp]
    public void Setup()
    {
        consoleMock = new Mock<IConsoleManager>();
    }

    [Test]
    public void SuccessfulCreateCLIAndRootCommand()
    {
        Action<ICommand, Options, ICommandLineInterface> testAction = (x, y, z) => { };

        CommandLineInterface cli = new CommandLineInterface(consoleMock.Object, metadata, testAction);
        Assert.AreEqual(metadata.Title, cli.Title);
        Assert.AreEqual(metadata.RootCommandName, cli.RootCommand.Name);
        Assert.AreEqual(metadata.Description, cli.RootCommand.Description);
        Assert.AreEqual(testAction, cli.RootCommand.Action);
    }

    [Test]
    public void CreateCLIWithNullConsoleError()
    {
        Assert.Throws<ArgumentNullException>(() => new CommandLineInterface(null, metadata, (x, y, z) => { }));
    }

    [Test]
    public void CreateCLIWithNullMetadataError()
    {
        Assert.Throws<ArgumentNullException>(() => new CommandLineInterface(consoleMock.Object, null, (x, y, z) => { }));
    }

    [Test]
    public void CreateCLIWithNullActionError()
    {
        Assert.Throws<ArgumentNullException>(() => new CommandLineInterface(consoleMock.Object, metadata, null));
    }

    [Test]
    public void SuccessfulCreateCommand()
    {
        Action<ICommand, Options, ICommandLineInterface> testAction = (x, y, z) => { };
        CommandLineInterface cli = new CommandLineInterface(consoleMock.Object, metadata, testAction);


        Action<ICommand, Options, ICommandLineInterface> newAction = (x, y, z) => { };
        string name = "new";
        string description = "new command for test";
        ICommand command = cli.CreateCommand(name, description, newAction);
    
        Assert.AreEqual(name, command.Name);
        Assert.AreEqual(description, command.Description);
        Assert.AreEqual(newAction, command.Action);
    }

    [Test]
    public void SuccessfulConfirmWithYesAnswer()
    {
        string answerExpected = "Y";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        CommandLineInterface cli = new CommandLineInterface(consoleMock.Object, metadata, (x, y, z) => { });

        string question = "Do you agree? (Y/n):";
        bool answer = cli.Confirm();

        consoleMock.Verify(x => x.Write(question), Times.Once);
        consoleMock.Verify(x => x.ReadLine(), Times.Once);
        
        Assert.IsTrue(answer);
    }

    [Test]
    public void SuccessfulConfirmWithNoAnswer()
    {
        string answerExpected = "n";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        CommandLineInterface cli = new CommandLineInterface(consoleMock.Object, metadata, (x, y, z) => { });

        string question = "Do you agree? (Y/n):";
        bool answer = cli.Confirm();

        consoleMock.Verify(x => x.Write(question), Times.Once);
        consoleMock.Verify(x => x.ReadLine(), Times.Once);
        
        Assert.IsFalse(answer);
    }

    [Test]
    public void SuccessfulConfirmWithOtherAnswer()
    {
        string answerExpected = "xxxxx";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        CommandLineInterface cli = new CommandLineInterface(consoleMock.Object, metadata, (x, y, z) => { });

        string question = "Do you agree? (Y/n):";
        bool answer = cli.Confirm();

        consoleMock.Verify(x => x.Write(question), Times.Once);
        consoleMock.Verify(x => x.ReadLine(), Times.Once);
        
        Assert.IsFalse(answer);
    }

    [Test]
    public void SuccessfulConfirmWithEmptyAnswer()
    {
        string answerExpected = "";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        CommandLineInterface cli = new CommandLineInterface(consoleMock.Object, metadata, (x, y, z) => { });

        string question = "Do you agree? (Y/n):";
        bool answer = cli.Confirm();

        consoleMock.Verify(x => x.Write(question), Times.Once);
        consoleMock.Verify(x => x.ReadLine(), Times.Once);
        
        Assert.IsFalse(answer);
    }

    [Test]
    public void SuccessfulConfirmWithCustomYesAnswer()
    {
        string answerExpected = "I'm agree";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        CommandLineInterface cli = new CommandLineInterface(consoleMock.Object, metadata, (x, y, z) => { });

        string question = "Do you agree? (I'm agree/n):";
        bool answer = cli.Confirm(yes: "I'm agree");

        consoleMock.Verify(x => x.Write(question), Times.Once);
        consoleMock.Verify(x => x.ReadLine(), Times.Once);
        
        Assert.IsTrue(answer);
    }

    [Test]
    public void SuccessfulConfirmWithCustomNoAnswer()
    {
        string answerExpected = "NO";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        CommandLineInterface cli = new CommandLineInterface(consoleMock.Object, metadata, (x, y, z) => { });

        string question = "Do you agree? (I'm agree/n):";
        bool answer = cli.Confirm(yes: "I'm agree");

        consoleMock.Verify(x => x.Write(question), Times.Once);
        consoleMock.Verify(x => x.ReadLine(), Times.Once);
        
        Assert.IsFalse(answer);
    }

    [Test]
    public void SuccessfulConfirmWithCustomQuestion()
    {
        string answerExpected = "Y";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        CommandLineInterface cli = new CommandLineInterface(consoleMock.Object, metadata, (x, y, z) => { });

        string question = "Are you kidding me? (Y/n):";
        bool answer = cli.Confirm(question: "Are you kidding me?");

        consoleMock.Verify(x => x.Write(question), Times.Once);
        consoleMock.Verify(x => x.ReadLine(), Times.Once);
        
        Assert.IsTrue(answer);
    }

    [Test]
    public void SuccessfulAskFor()
    {
        string answerExpected = "test answer";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        CommandLineInterface cli = new CommandLineInterface(consoleMock.Object, metadata, (x, y, z) => { });

        string question = "test question:";
        string answer = cli.AskFor("test question");

        consoleMock.Verify(x => x.Write(question), Times.Once);
        
        Assert.AreEqual(answerExpected, answer);
    }

    [Test]
    public void AskForWithNullError()
    {
        CommandLineInterface cli = new CommandLineInterface(consoleMock.Object, metadata, (x, y, z) => { });

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => cli.AskFor(null));

        Assert.IsTrue(exception?.Message.Contains("Value cannot be null"));

        consoleMock.Verify(x => x.Write(It.IsAny<string>()), Times.Never);
        consoleMock.Verify(x => x.ReadLine(), Times.Never);
    }

    [Test]
    public void AskForWithWhitespaceError()
    {
        CommandLineInterface cli = new CommandLineInterface(consoleMock.Object, metadata, (x, y, z) => { });

        string question = "     ";
        ArgumentException? exception = Assert.Throws<ArgumentException>(() => cli.AskFor(question));

        Assert.IsTrue(exception?.Message.Contains(CommandLineInterface.QUESTION_MUST_BE_NOT_BLANK));

        consoleMock.Verify(x => x.Write($"{question}:"), Times.Never);
        consoleMock.Verify(x => x.ReadLine(), Times.Never);
    }

    [Test]
    public void SuccessfulAskForSensitive()
    {
        string answerExpected = "x1A";
        consoleMock.Setup(x => x.ReadSensitive()).Returns(answerExpected);

        CommandLineInterface cli = new CommandLineInterface(consoleMock.Object, metadata, (x, y, z) => { });

        string question = "test question:";
        string answer = cli.AskForSensitive("test question");

        consoleMock.Verify(x => x.Write(question), Times.Once);
        
        Assert.AreEqual(answerExpected, answer);
    }

    [Test]
    public void SuccessfulExecuteRootWithNoArgs()
    {
        string[] args = new string[] { };

        bool actionExecuted = false;
        Action<ICommand, Options, ICommandLineInterface> testAction = (x, y, z) => { actionExecuted = true; };
        CommandLineInterface cli = new CommandLineInterface(consoleMock.Object, metadata, testAction);

        cli.Execute(args);
        
        Assert.IsTrue(actionExecuted);
    }

    [Test]
    public void SuccessfulExecuteRootWithEmptyArg()
    {
        string[] args = new string[] { "" };

        bool actionExecuted = false;
        Action<ICommand, Options, ICommandLineInterface> testAction = (x, y, z) => { actionExecuted = true; };
        CommandLineInterface cli = new CommandLineInterface(consoleMock.Object, metadata, testAction);

        cli.Execute(args);
        
        Assert.IsTrue(actionExecuted);
    }

    [Test]
    public void SuccessfulExecuteRootWithSpacesArg()
    {
        string[] args = new string[] { "  " };

        bool actionExecuted = false;
        Action<ICommand, Options, ICommandLineInterface> testAction = (x, y, z) => { actionExecuted = true; };
        CommandLineInterface cli = new CommandLineInterface(consoleMock.Object, metadata, testAction);

        cli.Execute(args);
        
        Assert.IsTrue(actionExecuted);
    }

    [Test]
    public void SuccessfulExecuteRootWithMultiSpacesArg()
    {
        string[] args = new string[] { "  ", "   ", "", "  " };

        bool actionExecuted = false;
        Action<ICommand, Options, ICommandLineInterface> testAction = (x, y, z) => { actionExecuted = true; };
        CommandLineInterface cli = new CommandLineInterface(consoleMock.Object, metadata, testAction);

        cli.Execute(args);
        
        Assert.IsTrue(actionExecuted);
    }

}