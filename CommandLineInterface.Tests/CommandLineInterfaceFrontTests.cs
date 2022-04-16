using System;
using System.Collections.Generic;
using CommandLineInterface;
using Moq;
using NUnit.Framework;

namespace CommandLineInterface.Tests;

public class CommandLineInterfaceFrontTests
{

    private readonly Metadata metadata = new("Testfront");
    private readonly Mock<IConsoleManager> consoleMock = new();

    [SetUp]
    public void Setup()
    {
        consoleMock.Reset();
    }

    [Test]
    public void SuccessfulConfirmWithYesAnswer()
    {
        string answerExpected = "Y";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        CommandLineInterfaceFront front = new(consoleMock.Object, metadata);

        string question = "Do you agree? (Y/n):";
        bool answer = front.Confirm();

        consoleMock.Verify(x => x.Write(question), Times.Once);
        consoleMock.Verify(x => x.ReadLine(), Times.Once);

        Assert.IsTrue(answer);
    }

    [Test]
    public void SuccessfulConfirmWithNoAnswer()
    {
        string answerExpected = "n";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        CommandLineInterfaceFront front = new(consoleMock.Object, metadata);

        string question = "Do you agree? (Y/n):";
        bool answer = front.Confirm();

        consoleMock.Verify(x => x.Write(question), Times.Once);
        consoleMock.Verify(x => x.ReadLine(), Times.Once);

        Assert.IsFalse(answer);
    }

    [Test]
    public void SuccessfulConfirmWithOtherAnswer()
    {
        string answerExpected = "xxxxx";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        CommandLineInterfaceFront front = new(consoleMock.Object, metadata);

        string question = "Do you agree? (Y/n):";
        bool answer = front.Confirm();

        consoleMock.Verify(x => x.Write(question), Times.Once);
        consoleMock.Verify(x => x.ReadLine(), Times.Once);

        Assert.IsFalse(answer);
    }

    [Test]
    public void SuccessfulConfirmWithEmptyAnswer()
    {
        string answerExpected = "";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        CommandLineInterfaceFront front = new(consoleMock.Object, metadata);

        string question = "Do you agree? (Y/n):";
        bool answer = front.Confirm();

        consoleMock.Verify(x => x.Write(question), Times.Once);
        consoleMock.Verify(x => x.ReadLine(), Times.Once);

        Assert.IsFalse(answer);
    }

    [Test]
    public void SuccessfulConfirmWithCustomYesAnswer()
    {
        string answerExpected = "I'm agree";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        CommandLineInterfaceFront front = new(consoleMock.Object, metadata);

        string question = "Do you agree? (I'm agree/n):";
        bool answer = front.Confirm(yes: "I'm agree");

        consoleMock.Verify(x => x.Write(question), Times.Once);
        consoleMock.Verify(x => x.ReadLine(), Times.Once);

        Assert.IsTrue(answer);
    }

    [Test]
    public void SuccessfulConfirmWithCustomNoAnswer()
    {
        string answerExpected = "NO";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        CommandLineInterfaceFront front = new(consoleMock.Object, metadata);

        string question = "Do you agree? (I'm agree/n):";
        bool answer = front.Confirm(yes: "I'm agree");

        consoleMock.Verify(x => x.Write(question), Times.Once);
        consoleMock.Verify(x => x.ReadLine(), Times.Once);

        Assert.IsFalse(answer);
    }

    [Test]
    public void SuccessfulConfirmWithCustomQuestion()
    {
        string answerExpected = "Y";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        CommandLineInterfaceFront front = new(consoleMock.Object, metadata);

        string question = "Are you kidding me? (Y/n):";
        bool answer = front.Confirm(question: "Are you kidding me?");

        consoleMock.Verify(x => x.Write(question), Times.Once);
        consoleMock.Verify(x => x.ReadLine(), Times.Once);

        Assert.IsTrue(answer);
    }

    [Test]
    public void SuccessfulAskFor()
    {
        string answerExpected = "test answer";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        CommandLineInterfaceFront front = new(consoleMock.Object, metadata);

        string question = "test question:";
        string answer = front.AskFor("test question");

        consoleMock.Verify(x => x.Write(question), Times.Once);

        Assert.AreEqual(answerExpected, answer);
    }


    [Test]
    public void AskForWithWhitespaceError()
    {
        CommandLineInterfaceFront front = new(consoleMock.Object, metadata);

        string question = "     ";
        ArgumentException? exception = Assert.Throws<ArgumentException>(() => front.AskFor(question));

        Assert.IsTrue(exception?.Message.Contains(CommandLineInterfaceFront.QUESTION_MUST_BE_NOT_BLANK));

        consoleMock.Verify(x => x.Write($"{question}:"), Times.Never);
        consoleMock.Verify(x => x.ReadLine(), Times.Never);
    }

    [Test]
    public void SuccessfulAskForSensitive()
    {
        string answerExpected = "x1A";
        consoleMock.Setup(x => x.ReadSensitive()).Returns(answerExpected);

        CommandLineInterfaceFront front = new(consoleMock.Object, metadata, true);

        string question = "test question:";
        string answer = front.AskForSensitive("test question");

        consoleMock.Verify(x => x.Write(question, 1), Times.Once);

        Assert.AreEqual(answerExpected, answer);
        Assert.AreEqual(1, front.PrintedLines);
    }
}