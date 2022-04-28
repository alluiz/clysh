using System;
using System.Linq;
using Clysh.Data;
using Moq;
using NUnit.Framework;

namespace Clysh.Tests;

public class ClyshViewTests
{

    private readonly ClyshData metadata = new ClyshData(title: "Auth 2 API", "1.0");
    private readonly Mock<IClyshConsole> consoleMock = new();

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

        IClyshView view = new ClyshView(consoleMock.Object, metadata, true);

        string question = "Do you agree? (Y/n):";
        bool answer = view.Confirm();

        consoleMock.Verify(x => x.Write(question, 1), Times.Once);
        consoleMock.Verify(x => x.ReadLine(), Times.Once);

        Assert.IsTrue(answer);
        Assert.AreEqual(1, view.PrintedLines);
    }

    [Test]
    public void SuccessfulConfirmWithNoAnswer()
    {
        string answerExpected = "n";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        IClyshView view = new ClyshView(consoleMock.Object, metadata, true);

        string question = "Do you agree? (Y/n):";
        bool answer = view.Confirm();

        consoleMock.Verify(x => x.Write(question, 1), Times.Once);
        consoleMock.Verify(x => x.ReadLine(), Times.Once);

        Assert.IsFalse(answer);
        Assert.AreEqual(1, view.PrintedLines);
    }

    [Test]
    public void SuccessfulConfirmWithOtherAnswer()
    {
        string answerExpected = "xxxxx";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        IClyshView view = new ClyshView(consoleMock.Object, metadata, true);

        string question = "Do you agree? (Y/n):";
        bool answer = view.Confirm();

        consoleMock.Verify(x => x.Write(question, 1), Times.Once);
        consoleMock.Verify(x => x.ReadLine(), Times.Once);

        Assert.IsFalse(answer);
        Assert.AreEqual(1, view.PrintedLines);
    }

    [Test]
    public void SuccessfulConfirmWithEmptyAnswer()
    {
        string answerExpected = "";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        IClyshView view = new ClyshView(consoleMock.Object, metadata, true);

        string question = "Do you agree? (Y/n):";
        bool answer = view.Confirm();

        consoleMock.Verify(x => x.Write(question, 1), Times.Once);
        consoleMock.Verify(x => x.ReadLine(), Times.Once);

        Assert.IsFalse(answer);
        Assert.AreEqual(1, view.PrintedLines);
    }

    [Test]
    public void SuccessfulConfirmWithCustomYesAnswer()
    {
        string answerExpected = "I'm agree";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        IClyshView view = new ClyshView(consoleMock.Object, metadata, true);

        string question = "Do you agree? (I'm agree/n):";
        bool answer = view.Confirm(yes: "I'm agree");

        consoleMock.Verify(x => x.Write(question, 1), Times.Once);
        consoleMock.Verify(x => x.ReadLine(), Times.Once);

        Assert.IsTrue(answer);
        Assert.AreEqual(1, view.PrintedLines);
    }

    [Test]
    public void SuccessfulConfirmWithCustomNoAnswer()
    {
        string answerExpected = "NO";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        IClyshView view = new ClyshView(consoleMock.Object, metadata, true);

        string question = "Do you agree? (I'm agree/n):";
        bool answer = view.Confirm(yes: "I'm agree");

        consoleMock.Verify(x => x.Write(question, 1), Times.Once);
        consoleMock.Verify(x => x.ReadLine(), Times.Once);

        Assert.IsFalse(answer);
        Assert.AreEqual(1, view.PrintedLines);
    }

    [Test]
    public void SuccessfulConfirmWithCustomQuestion()
    {
        string answerExpected = "Y";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        IClyshView view = new ClyshView(consoleMock.Object, metadata, true);

        string question = "Are you kidding me? (Y/n):";
        bool answer = view.Confirm("Are you kidding me?");

        consoleMock.Verify(x => x.Write(question, 1), Times.Once);
        consoleMock.Verify(x => x.ReadLine(), Times.Once);

        Assert.IsTrue(answer);
        Assert.AreEqual(1, view.PrintedLines);
    }

    [Test]
    public void SuccessfulAskFor()
    {
        string answerExpected = "test answer";
        consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        IClyshView view = new ClyshView(consoleMock.Object, metadata, true);

        string question = "test question:";
        string answer = view.AskFor("test question");

        consoleMock.Verify(x => x.Write(question, 1), Times.Once);

        Assert.AreEqual(answerExpected, answer);
        Assert.AreEqual(1, view.PrintedLines);
    }

    [Test]
    public void SuccessfulAskForSensitive()
    {
        string answerExpected = "x1A";
        consoleMock.Setup(x => x.ReadSensitive()).Returns(answerExpected);

        IClyshView view = new ClyshView(consoleMock.Object, metadata, true);

        string question = "test question:";
        string answer = view.AskForSensitive("test question");

        consoleMock.Verify(x => x.Write(question, 1), Times.Once);

        Assert.AreEqual(answerExpected, answer);
        Assert.AreEqual(1, view.PrintedLines);
    }

    [Test]
    public void AskForWithWhitespaceError()
    {
        IClyshView view = new ClyshView(consoleMock.Object, metadata, true);

        string question = "     ";
        ArgumentException? exception = Assert.Throws<ArgumentException>(() => view.AskFor(question));

        Assert.IsTrue(exception?.Message.Contains(ClyshView.QUESTION_MUST_BE_NOT_BLANK));

        consoleMock.Verify(x => x.Write($"{question}:"), Times.Never);
        consoleMock.Verify(x => x.ReadLine(), Times.Never);
        Assert.AreEqual(0, view.PrintedLines);
    }

    [Test]
    public void SuccessfulPrintRootCommandHelp()
    {
        IClyshView view = new ClyshView(consoleMock.Object, new ClyshData(title: "Auth 2 API", "1.0"), true);

        IClyshCommand command = ClyshDataForTest.CreateRootCommand();
        view.PrintHelp(command);

        consoleMock.Verify(x => x.WriteLine("", 1), Times.Once);
        consoleMock.Verify(x => x.WriteLine($"{view.Data.Title}. Version: {view.Data.Version}", 2), Times.Once);
        consoleMock.Verify(x => x.WriteLine("", 3), Times.Once);
        consoleMock.Verify(x => x.WriteLine("Usage: auth2 [options] [commands]", 4), Times.Once);
        consoleMock.Verify(x => x.WriteLine("", 5), Times.Once);
        consoleMock.Verify(x => x.WriteLine(command.Description, 6), Times.Once);
        consoleMock.Verify(x => x.WriteLine("", 7), Times.Once);
        consoleMock.Verify(x => x.WriteLine("[options]:", 8), Times.Once);
        consoleMock.Verify(x => x.WriteLine("", 9), Times.Once);
        consoleMock.Verify(x => x.WriteLine("".PadRight(3) + "Shortcut".PadRight(11) + "Option".PadRight(28) + "Description".PadRight(55) + "Parameters: (R)equired | (O)ptional = Length", 10), Times.Once);
        consoleMock.Verify(x => x.WriteLine("", 11), Times.Once);

        int i = 0;

        foreach (var item in command.AvailableOptions.Itens.OrderBy(x => x.Key))
        {
            consoleMock.Verify(x => x.WriteLine("".PadRight(2) + $"{(item.Value.Shortcut == null ? "" : "-" + item.Value.Shortcut),-10}--{item.Key,-28}{item.Value.Description,-55}{item.Value.Parameters}", 12 + i), Times.Once);
            i++;
        }

        //i=4

        consoleMock.Verify(x => x.WriteLine("", 12 + i), Times.Once);
        consoleMock.Verify(x => x.WriteLine("[commands]:", 13 + i), Times.Once);
        consoleMock.Verify(x => x.WriteLine("", 14 + i), Times.Once);

        int j = i + 1;
        foreach (var item in command.Commands.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value))
        {
            if (item.Key != command.Id)
            {
                consoleMock.Verify(x => x.WriteLine("".PadRight(3) + $"{item.Key,-39}{item.Value.Description}", 14 + j), Times.Once);
                j++;
            }
        }

        //j=7

        consoleMock.Verify(x => x.WriteLine("", 14 + j), Times.Once);

        Assert.AreEqual(21, view.PrintedLines);
    }

    [Test]
    public void SuccessfulPrintCustomCommandHelp()
    {
        IClyshView view = new ClyshView(consoleMock.Object, metadata, true);

        IClyshCommand command = ClyshDataForTest.CreateRootCommand().GetCommand("credential");
        view.PrintHelp(command);

        consoleMock.Verify(x => x.WriteLine("", 1), Times.Once);
        consoleMock.Verify(x => x.WriteLine($"{view.Data.Title}. Version: {view.Data.Version}", 2), Times.Once);
        consoleMock.Verify(x => x.WriteLine("", 3), Times.Once);
        consoleMock.Verify(x => x.WriteLine("Usage: auth2 credential [options] [commands]", 4), Times.Once);
        consoleMock.Verify(x => x.WriteLine("", 5), Times.Once);
        consoleMock.Verify(x => x.WriteLine(command.Description, 6), Times.Once);
        consoleMock.Verify(x => x.WriteLine("", 7), Times.Once);
        consoleMock.Verify(x => x.WriteLine("[options]:", 8), Times.Once);
        consoleMock.Verify(x => x.WriteLine("", 9), Times.Once);
        consoleMock.Verify(x => x.WriteLine("".PadRight(3) + "Shortcut".PadRight(11) + "Option".PadRight(28) + "Description".PadRight(55) + "Parameters: (R)equired | (O)ptional = Length", 10), Times.Once);
        consoleMock.Verify(x => x.WriteLine("", 11), Times.Once);

        int i = 0;

        foreach (var item in command.AvailableOptions.Itens.OrderBy(x => x.Key))
        {
            consoleMock.Verify(x => x.WriteLine("".PadRight(2) + $"{(item.Value.Shortcut == null ? "" : "-" + item.Value.Shortcut),-10}--{item.Key,-28}{item.Value.Description,-55}{item.Value.Parameters}", 12 + i), Times.Once);
            i++;
        }

        //i=4

        consoleMock.Verify(x => x.WriteLine("", 12 + i), Times.Once);
        consoleMock.Verify(x => x.WriteLine("[commands]:", 13 + i), Times.Once);
        consoleMock.Verify(x => x.WriteLine("", 14 + i), Times.Once);

        int j = i + 1;
        foreach (var item in command.Commands.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value))
        {
            if (item.Key != command.Id)
            {
                consoleMock.Verify(x => x.WriteLine("".PadRight(3) + $"{item.Key,-39}{item.Value.Description}", 14 + j), Times.Once);
                j++;
            }
        }

        //j=7

        consoleMock.Verify(x => x.WriteLine("", 14 + j), Times.Once);

        Assert.AreEqual(19, view.PrintedLines);
    }

    [Test]
    public void SuccessfulPrintCustomCommandTreeHelp()
    {
        IClyshView view = new ClyshView(consoleMock.Object, metadata, true);

        IClyshCommand command = ClyshDataForTest.CreateRootCommand().GetCommand("credential").GetCommand("test");
        view.PrintHelp(command);

        consoleMock.Verify(x => x.WriteLine("", 1), Times.Once);
        consoleMock.Verify(x => x.WriteLine($"{view.Data.Title}. Version: {view.Data.Version}", 2), Times.Once);
        consoleMock.Verify(x => x.WriteLine("", 3), Times.Once);
        consoleMock.Verify(x => x.WriteLine("Usage: auth2 credential test [options]", 4), Times.Once);
        consoleMock.Verify(x => x.WriteLine("", 5), Times.Once);
        consoleMock.Verify(x => x.WriteLine(command.Description, 6), Times.Once);
        consoleMock.Verify(x => x.WriteLine("", 7), Times.Once);
        consoleMock.Verify(x => x.WriteLine("[options]:", 8), Times.Once);
        consoleMock.Verify(x => x.WriteLine("", 9), Times.Once);
        consoleMock.Verify(x => x.WriteLine("".PadRight(3) + "Shortcut".PadRight(11) + "Option".PadRight(28) + "Description".PadRight(55) + "Parameters: (R)equired | (O)ptional = Length", 10), Times.Once);
        consoleMock.Verify(x => x.WriteLine("", 11), Times.Once);

        int i = 0;

        foreach (var item in command.AvailableOptions.Itens.OrderBy(x => x.Key))
        {
            consoleMock.Verify(x => x.WriteLine("".PadRight(2) + $"{(item.Value.Shortcut == null ? "" : "-" + item.Value.Shortcut),-10}--{item.Key,-28}{item.Value.Description,-55}{item.Value.Parameters}", 12 + i), Times.Once);
            i++;
        }

        //i=4

        consoleMock.Verify(x => x.WriteLine("", 12 + i), Times.Once);

        Assert.AreEqual(14, view.PrintedLines);
    }

    [Test]
    public void SuccessfulPrintRootCommandWithExceptionHelp()
    {
        IClyshView view = new ClyshView(consoleMock.Object, metadata, true);

        IClyshCommand rootCommand = ClyshDataForTest.CreateRootCommand();

        try
        {
            throw new Exception("Test Exception");
        }
        catch (Exception exception)
        {
            view.PrintHelp(rootCommand, exception);
        }

        consoleMock.Verify(x => x.WriteLine("", 1), Times.Once);
        consoleMock.Verify(x => x.WriteLine("-----------#-----------", 2), Times.Once);
        consoleMock.Verify(x => x.WriteLine("", 3), Times.Once);
        consoleMock.Verify(x => x.WriteLine($"Error: Exception: Test Exception", 4), Times.Once);
        consoleMock.Verify(x => x.WriteLine("", 5), Times.Once);
        consoleMock.Verify(x => x.WriteLine("-----------#-----------", 6), Times.Once);

        Assert.AreEqual(27, view.PrintedLines);
    }
}