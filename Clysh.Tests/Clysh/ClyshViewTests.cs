using System;
using System.Linq;
using Clysh.Core;
using Clysh.Data;
using Clysh.Helper;
using Moq;
using NUnit.Framework;

namespace Clysh.Tests;

public class ClyshViewTests
{
    private readonly Mock<IClyshConsole> _consoleMock = new();

    private readonly ClyshData _metadata = new(title: "Auth 2 API", "1.0");

    [SetUp]
    public void Setup()
    {
        _consoleMock.Reset();
    }

    [Test]
    public void SuccessfulConfirmWithYesAnswer()
    {
        var answerExpected = "Y";
        _consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        IClyshView view = new ClyshView(_consoleMock.Object, _metadata, true);

        var question = "Do you agree? (Y/n):";
        var answer = view.Confirm();

        _consoleMock.Verify(x => x.Write(question, 1), Times.Once);
        _consoleMock.Verify(x => x.ReadLine(), Times.Once);

        Assert.IsTrue(answer);
        Assert.AreEqual(1, view.PrintedLines);
    }

    [Test]
    public void SuccessfulConfirmWithNoAnswer()
    {
        var answerExpected = "n";
        _consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        IClyshView view = new ClyshView(_consoleMock.Object, _metadata, true);

        var question = "Do you agree? (Y/n):";
        var answer = view.Confirm();

        _consoleMock.Verify(x => x.Write(question, 1), Times.Once);
        _consoleMock.Verify(x => x.ReadLine(), Times.Once);

        Assert.IsFalse(answer);
        Assert.AreEqual(1, view.PrintedLines);
    }

    [Test]
    public void SuccessfulConfirmWithOtherAnswer()
    {
        var answerExpected = "xxxxx";
        _consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        IClyshView view = new ClyshView(_consoleMock.Object, _metadata, true);

        var question = "Do you agree? (Y/n):";
        var answer = view.Confirm();

        _consoleMock.Verify(x => x.Write(question, 1), Times.Once);
        _consoleMock.Verify(x => x.ReadLine(), Times.Once);

        Assert.IsFalse(answer);
        Assert.AreEqual(1, view.PrintedLines);
    }

    [Test]
    public void SuccessfulConfirmWithEmptyAnswer()
    {
        var answerExpected = "";
        _consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        IClyshView view = new ClyshView(_consoleMock.Object, _metadata, true);

        var question = "Do you agree? (Y/n):";
        var answer = view.Confirm();

        _consoleMock.Verify(x => x.Write(question, 1), Times.Once);
        _consoleMock.Verify(x => x.ReadLine(), Times.Once);

        Assert.IsFalse(answer);
        Assert.AreEqual(1, view.PrintedLines);
    }

    [Test]
    public void SuccessfulConfirmWithCustomYesAnswer()
    {
        var answerExpected = "I'm agree";
        _consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        IClyshView view = new ClyshView(_consoleMock.Object, _metadata, true);

        var question = "Do you agree? (I'm agree/n):";
        var answer = view.Confirm(yes: "I'm agree");

        _consoleMock.Verify(x => x.Write(question, 1), Times.Once);
        _consoleMock.Verify(x => x.ReadLine(), Times.Once);

        Assert.IsTrue(answer);
        Assert.AreEqual(1, view.PrintedLines);
    }

    [Test]
    public void SuccessfulConfirmWithCustomNoAnswer()
    {
        var answerExpected = "NO";
        _consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        IClyshView view = new ClyshView(_consoleMock.Object, _metadata, true);

        var question = "Do you agree? (I'm agree/n):";
        var answer = view.Confirm(yes: "I'm agree");

        _consoleMock.Verify(x => x.Write(question, 1), Times.Once);
        _consoleMock.Verify(x => x.ReadLine(), Times.Once);

        Assert.IsFalse(answer);
        Assert.AreEqual(1, view.PrintedLines);
    }

    [Test]
    public void SuccessfulConfirmWithCustomQuestion()
    {
        var answerExpected = "Y";
        _consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        IClyshView view = new ClyshView(_consoleMock.Object, _metadata, true);

        var question = "Are you kidding me? (Y/n):";
        var answer = view.Confirm("Are you kidding me?");

        _consoleMock.Verify(x => x.Write(question, 1), Times.Once);
        _consoleMock.Verify(x => x.ReadLine(), Times.Once);

        Assert.IsTrue(answer);
        Assert.AreEqual(1, view.PrintedLines);
    }

    [Test]
    public void SuccessfulAskFor()
    {
        var answerExpected = "test answer";
        _consoleMock.Setup(x => x.ReadLine()).Returns(answerExpected);

        IClyshView view = new ClyshView(_consoleMock.Object, _metadata, true);

        var question = "test question:";
        var answer = view.AskFor("test question");

        _consoleMock.Verify(x => x.Write(question, 1), Times.Once);

        Assert.AreEqual(answerExpected, answer);
        Assert.AreEqual(1, view.PrintedLines);
    }

    [Test]
    public void SuccessfulAskForSensitive()
    {
        var answerExpected = "x1A";
        _consoleMock.Setup(x => x.ReadSensitive()).Returns(answerExpected);

        IClyshView view = new ClyshView(_consoleMock.Object, _metadata, true);

        var question = "test question:";
        var answer = view.AskForSensitive("test question");

        _consoleMock.Verify(x => x.Write(question, 1), Times.Once);

        Assert.AreEqual(answerExpected, answer);
        Assert.AreEqual(1, view.PrintedLines);
    }

    [Test]
    public void AskForWithWhitespaceError()
    {
        IClyshView view = new ClyshView(_consoleMock.Object, _metadata, true);

        var question = "     ";
        var exception = Assert.Throws<ArgumentException>(() => view.AskFor(question));

        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateUserInputQuestionAnswer);
        
        _consoleMock.Verify(x => x.Write($"{question}:"), Times.Never);
        _consoleMock.Verify(x => x.ReadLine(), Times.Never);
        Assert.AreEqual(0, view.PrintedLines);
    }

    [Test]
    public void SuccessfulPrintRootCommandHelp()
    {
        IClyshView view = new ClyshView(_consoleMock.Object, _metadata, true);

        var command = ClyshDataForTest.CreateRootCommand();
        view.PrintHelp(command);

        VerifyHelpHeaders(command);

        var i = 0;

        foreach (var option in command.Options
                     .OrderBy(x => x.Value.Group?.Id)
                     .ThenBy(y=>y.Key)
                     .Select(z => z.Value))
        {
            var i1 = i;
            
            const int maxDescriptionlengthPerLine = 30;

            var description = option.Description;

            var truncate = description.Length > maxDescriptionlengthPerLine;

            var firstDescriptionLine = truncate ? description[..maxDescriptionlengthPerLine] : description;
            
            _consoleMock.Verify(x => x.WriteLine($"{"",-3}{option,-22}{option.Group,-11}{firstDescriptionLine,-35}{option.Parameters}", 12 + i1), Times.Once);

            i++;

            if (!truncate) 
                continue;
            
            var startIndex = maxDescriptionlengthPerLine;

            var numberOfLines = description.Length / maxDescriptionlengthPerLine;

            for (var line = 1; line <= numberOfLines; line++)
            {
                var index = startIndex;
                
                _consoleMock.Verify(x => x.WriteLine((description.Substring(index).Length < maxDescriptionlengthPerLine
                    ? $"{string.Empty,-36}{description.Substring(index)}"
                    : $"{string.Empty,-36}{description.Substring(index, maxDescriptionlengthPerLine)}"), 12 + i1 + line), Times.Once);

                startIndex = maxDescriptionlengthPerLine * (line + 1);
                
                i++;
            }
        }

        //i=4

        _consoleMock.Verify(x => x.WriteLine("", 12 + i), Times.Once);
        _consoleMock.Verify(x => x.WriteLine("[subcommands]:", 13 + i), Times.Once);
        _consoleMock.Verify(x => x.WriteLine("", 14 + i), Times.Once);

        var j = i + 1;
        foreach (var item in command.SubCommands.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value))
        {
            if (item.Key != command.Id)
            {
                var j1 = j;
                _consoleMock.Verify(x => x.WriteLine("".PadRight(3) + $"{item.Value.Name,-39}{item.Value.Description}", 14 + j1), Times.Once);
                j++;
            }
        }

        //j=7

        _consoleMock.Verify(x => x.WriteLine("", 14 + j), Times.Once);

        Assert.AreEqual(25, view.PrintedLines);
    }

    private void VerifyHelpHeaders(ClyshCommand command)
    {
        var subcommandsText = command.AnySubcommand()?" [subcommands]":"";
        
        _consoleMock.Verify(x => x.WriteLine("", 1), Times.Once);
        _consoleMock.Verify(x => x.WriteLine($"{_metadata.Title}. Version: {_metadata.Version}", 2), Times.Once);
        _consoleMock.Verify(x => x.WriteLine("", 3), Times.Once);
        _consoleMock.Verify(x => x.WriteLine($"Usage: {command.Id.Replace(".", " ")} [options]{subcommandsText}", 4), Times.Once);
        _consoleMock.Verify(x => x.WriteLine("", 5), Times.Once);
        _consoleMock.Verify(x => x.WriteLine(command.Description, 6), Times.Once);
        _consoleMock.Verify(x => x.WriteLine("", 7), Times.Once);
        _consoleMock.Verify(x => x.WriteLine("[options]:", 8), Times.Once);
        _consoleMock.Verify(x => x.WriteLine("", 9), Times.Once);
        _consoleMock.Verify(
            x => x.WriteLine($"{"",-3}{"Option",-22}{"Group",-11}{"Description",-35}{"Parameters",-29}", 10),
            Times.Once);
        _consoleMock.Verify(x => x.WriteLine("", 11), Times.Once);
    }

    [Test]
    public void SuccessfulPrintCustomCommandHelp()
    {
        IClyshView view = new ClyshView(_consoleMock.Object, _metadata, true);

        var command = ClyshDataForTest.CreateRootCommand().SubCommands["auth2.credential"];
        view.PrintHelp(command);

        VerifyHelpHeaders(command);

        var i = 0;

        foreach (var option in command.Options.OrderBy(x => x.Value.Group?.Id)
                     .ThenBy(y => y.Key)
                     .Select(z => z.Value))
        {
            var i1 = i;
            _consoleMock.Verify(x => x.WriteLine("".PadRight(3) + $"{option,-33}{option.Description,-35}{option.Parameters}", 12 + i1), Times.Once);
            i++;
        }

        //i=4

        _consoleMock.Verify(x => x.WriteLine("", 12 + i), Times.Once);
        _consoleMock.Verify(x => x.WriteLine("[subcommands]:", 13 + i), Times.Once);
        _consoleMock.Verify(x => x.WriteLine("", 14 + i), Times.Once);

        var j = i + 1;
        foreach (var item in command.SubCommands.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value))
        {
            if (item.Key != command.Id)
            {
                var j1 = j;
                _consoleMock.Verify(x => x.WriteLine("".PadRight(3) + $"{item.Value.Name,-39}{item.Value.Description}", 14 + j1), Times.Once);
                j++;
            }
        }

        //j=7

        _consoleMock.Verify(x => x.WriteLine("", 14 + j), Times.Once);

        Assert.AreEqual(21, view.PrintedLines);
    }

    [Test]
    public void SuccessfulPrintCustomCommandTreeHelp()
    {
        IClyshView view = new ClyshView(_consoleMock.Object, _metadata, true);

        var command = ClyshDataForTest.CreateRootCommand().SubCommands["auth2.credential"].SubCommands["auth2.credential.test"];
        view.PrintHelp(command);

        VerifyHelpHeaders(command);

        var i = 0;

        foreach (var option in command.Options.OrderBy(x => x.Value.Group?.Id)
                     .ThenBy(y => y.Key)
                     .Select(z => z.Value))
        {
            var i1 = i;
            var groupId = option.Group?.Id;
            var truncate = option.Description.Length > 30;
            var firstDescriptionLine = truncate ? option.Description[..30] : option.Description;
            _consoleMock.Verify(x => x.WriteLine("".PadRight(3) + $"{option, -22}{groupId,-11}{firstDescriptionLine,-35}{option.Parameters}", 12 + i1), Times.Once);
            i++;
        }

        //i=4

        _consoleMock.Verify(x => x.WriteLine("", 12 + i), Times.Once);

        Assert.AreEqual(16, view.PrintedLines);
    }

    [Test]
    public void SuccessfulPrintRootCommandWithExceptionHelp()
    {
        IClyshView view = new ClyshView(_consoleMock.Object, _metadata, true);

        try
        {
            throw new Exception("Test Exception");
        }
        catch (Exception exception)
        {
            view.PrintException(exception);
        }

        _consoleMock.Verify(x => x.WriteLine("", 1), Times.Once);
        _consoleMock.Verify(x => x.WriteLine($"Error: Test Exception", 2), Times.Once);
        _consoleMock.Verify(x => x.WriteLine("", 3), Times.Once);
        
        Assert.AreEqual(3, view.PrintedLines);
    }

    [Test]
    public void SuccessfulPrintWithoutLineNumber()
    {
        IClyshView view = new ClyshView(_consoleMock.Object, _metadata);

        var question = "test question:";
        
        view.Print(question);

        _consoleMock.Verify(x => x.WriteLine(question), Times.Once);

        Assert.AreEqual(1, view.PrintedLines);
    }

    [Test]
    public void SuccessfulPrintWithoutLineNumberAndNoBreak()
    {
        IClyshView view = new ClyshView(_consoleMock.Object, _metadata);

        var question = "test question:";
        
        view.PrintWithoutBreak(question);

        _consoleMock.Verify(x => x.Write(question), Times.Once);

        Assert.AreEqual(1, view.PrintedLines);
    }

    [Test]
    public void ShouldPrintAlert()
    {
        IClyshView view = new ClyshView(_consoleMock.Object, _metadata);

        const string text = "my text";
        
        view.PrintAlert(text);

        _consoleMock.Verify(x => x.WriteLine(text), Times.Once);

        Assert.AreEqual(1, view.PrintedLines);
    }
    
    [Test]
    public void ShouldPrintDebug()
    {
        IClyshView view = new ClyshView(_consoleMock.Object, _metadata);
        view.Debug = true;

        const string text = "my debug text";
        
        view.PrintDebug(text);

        _consoleMock.Verify(x => x.WriteLine(text), Times.Once);

        Assert.AreEqual(1, view.PrintedLines);
    }
    
    [Test]
    public void ShouldNotPrintDebug()
    {
        IClyshView view = new ClyshView(_consoleMock.Object, _metadata);
        view.Debug = false;

        const string text = "my debug text";
        
        view.PrintDebug(text);

        _consoleMock.Verify(x => x.WriteLine(text), Times.Never);

        Assert.AreEqual(0, view.PrintedLines);
    }
    
    [Test]
    public void ShouldPrintSeparator()
    {
        IClyshView view = new ClyshView(_consoleMock.Object, _metadata);

        view.PrintSeparator();

        _consoleMock.Verify(x => x.WriteLine(It.IsRegex("#+")), Times.Once);

        Assert.AreEqual(1, view.PrintedLines);
    }
    
    [Test]
    public void ShouldPrintSeparatorWithCustomChar()
    {
        IClyshView view = new ClyshView(_consoleMock.Object, _metadata);

        view.PrintSeparator("$");

        _consoleMock.Verify(x => x.WriteLine(It.IsRegex("$+")), Times.Once);

        Assert.AreEqual(1, view.PrintedLines);
    }
    
    [Test]
    public void ShouldPrintSuccess()
    {
        IClyshView view = new ClyshView(_consoleMock.Object, _metadata);

        const string text = "my text";
        
        view.PrintSuccess(text);

        _consoleMock.Verify(x => x.WriteLine(text), Times.Once);

        Assert.AreEqual(1, view.PrintedLines);
    }
}