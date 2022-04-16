using System;
using System.Collections.Generic;
using CommandLineInterface;
using Moq;
using NUnit.Framework;

namespace CommandLineInterface.Tests;

public class CommandLineInterfaceTests
{
    private readonly Mock<ICommandLineInterfaceFront> frontMock = new();
    private readonly Mock<ICommand> rootCommandMock = new();


    [SetUp]
    public void Setup()
    {
        frontMock.Reset();
        rootCommandMock.Reset();
    }

    [Test]
    public void SuccessfulCreateCLIAndRootCommand()
    {
        ICommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);
        Assert.AreEqual(rootCommandMock.Object, cli.RootCommand);
        Assert.AreEqual(frontMock.Object, cli.Front);
    }

    [Test]
    public void SuccessfulCreateCLIAndFront()
    {
        IConsoleManager consoleManager = new ConsoleManager();
        Metadata metadata = new(title: "Test");
        ICommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, consoleManager, metadata);
        Assert.AreEqual(rootCommandMock.Object, cli.RootCommand);
        Assert.AreEqual(metadata, cli.Front.Metadata);
    }

    [Test]
    public void SuccessfulCreateCommand()
    {
        ICommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

        Action<Map<Option>, ICommandLineInterfaceFront> newAction = (x, y) => { };
        string name = "new";
        string description = "new command for test";
        ICommand command = Command.Create(name, description, newAction);

        Assert.AreEqual(name, command.Name);
        Assert.AreEqual(description, command.Description);
        Assert.AreEqual(newAction, command.Action);
    }



    [Test]
    public void SuccessfulExecuteRootWithNoArgs()
    {
        string[] args = Array.Empty<string>();

        ICommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

        cli.Execute(args);

        rootCommandMock.Verify(x => x.Action, Times.Once);
    }

    [Test]
    public void SuccessfulExecuteRootWithOneEmptyArg()
    {
        string[] args = new string[] { "" };

        ICommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

        cli.Execute(args);

        rootCommandMock.Verify(x => x.Action, Times.Once);
    }

    [Test]
    public void SuccessfulExecuteRootWithOneSpaceArg()
    {
        string[] args = new string[] { "  " };

        ICommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

        cli.Execute(args);

        rootCommandMock.Verify(x => x.Action, Times.Once);
    }

    [Test]
    public void SuccessfulExecuteRootWithMultiSpacesArg()
    {
        string[] args = new string[] { "  ", "   ", "         ", "  " };

        ICommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

        cli.Execute(args);

        rootCommandMock.Verify(x => x.Action, Times.Once);

    }

    [Test]
    public void SuccessfulExecuteRootWithSomeOption()
    {
        const string someOption = "some-option";
        const string someAbbrevOption = "s";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        string[] args = new string[] { someOptionWithDashes };

        Map<Option>? expectedOptions = null;
        ICommandLineInterfaceFront? expectedCliFront = null;

        void action(Map<Option> options, ICommandLineInterfaceFront cliFront)
        {
            expectedOptions = options;
            expectedCliFront = cliFront;
        }

        ICommand rootCommand = Command.Create("root", "root command", action);

        ICommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

        cli.RootCommand
            .AddOption(someOption, someOptionDescription, someAbbrevOption);

        cli.Execute(args);

        Assert.NotNull(expectedOptions);
        Assert.NotNull(expectedCliFront);

        Assert.AreEqual(1, expectedOptions?.Itens.Count);
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?.Get(someOption).Id);
        Assert.AreEqual(someAbbrevOption, expectedOptions?.Get(someOption).Abbreviation);
        Assert.AreEqual(someOptionDescription, expectedOptions?.Get(someOption).Description);

        Assert.AreEqual(cli.Front, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithSomeOptionAndParameter()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        string[] args = new string[] { someOptionWithDashes, "testarg:mytest" };

        Map<Option>? expectedOptions = null;
        ICommandLineInterfaceFront? expectedCliFront = null;

        void action(Map<Option> options, ICommandLineInterfaceFront cliFront)
        {
            expectedOptions = options;
            expectedCliFront = cliFront;
        }

        ICommand rootCommand = Command.Create("root", "root command", action);

        ICommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

        cli.RootCommand
            .AddOption(someOption, someOptionDescription, Parameters.Create(new Parameter("testarg", 6, 10)));

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Itens.Count);
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?.Get(someOption).Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?.Get(someOption).Description);
        Assert.AreEqual("mytest", expectedOptions?.Get(someOption).Parameters.Get("testarg").Data);

        Assert.AreEqual(cli.Front, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithSomeOptionAndParameterByPosition()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        string[] args = new string[] { someOptionWithDashes, "mytest" };

        Map<Option>? expectedOptions = null;
        ICommandLineInterfaceFront? expectedCliFront = null;

        void action(Map<Option> options, ICommandLineInterfaceFront cliFront)
        {
            expectedOptions = options;
            expectedCliFront = cliFront;
        }

        ICommand rootCommand = Command.Create("root", "root command", action);

        ICommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

        cli.RootCommand
            .AddOption(someOption, someOptionDescription, Parameters.Create(new Parameter("testarg", 6, 10)));

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Itens.Count);
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?.Get(someOption).Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?.Get(someOption).Description);
        Assert.AreEqual("mytest", expectedOptions?.Get(someOption).Parameters.Get("testarg").Data);

        Assert.AreEqual(cli.Front, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithSomeOptionAndMultiParameter()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        string[] args = new string[] { someOptionWithDashes, "testarg:mytest", "testarg2:mytest2", "testarg3:mytest3" };

        Map<Option>? expectedOptions = null;
        ICommandLineInterfaceFront? expectedCliFront = null;

        void action(Map<Option> options, ICommandLineInterfaceFront cliFront)
        {
            expectedOptions = options;
            expectedCliFront = cliFront;
        }

        ICommand rootCommand = Command.Create("root", "root command", action);

        ICommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

        cli.RootCommand
            .AddOption(someOption, someOptionDescription, Parameters.Create(new("testarg", 6, 10)
                , new("testarg2", 6, 10)
                , new("testarg3", 6, 10)));

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Itens.Count);
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?.Get(someOption).Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?.Get(someOption).Description);
        Assert.AreEqual("mytest", expectedOptions?.Get(someOption).Parameters.Get("testarg").Data);
        Assert.AreEqual("mytest2", expectedOptions?.Get(someOption).Parameters.Get("testarg2").Data);
        Assert.AreEqual("mytest3", expectedOptions?.Get(someOption).Parameters.Get("testarg3").Data);

        Assert.AreEqual(cli.Front, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithSomeOptionAndOptionalParameter()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        string[] args = new string[] { someOptionWithDashes, "testarg:mytest" };

        Map<Option>? expectedOptions = null;
        ICommandLineInterfaceFront? expectedCliFront = null;

        void action(Map<Option> options, ICommandLineInterfaceFront cliFront)
        {
            expectedOptions = options;
            expectedCliFront = cliFront;
        }

        ICommand rootCommand = Command.Create("root", "root command", action);

        ICommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

        cli.RootCommand
            .AddOption(someOption, someOptionDescription, Parameters.Create(new Parameter("testarg", 6, 10, false)));

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Itens.Count);
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?.Get(someOption).Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?.Get(someOption).Description);
        Assert.AreEqual("mytest", expectedOptions?.Get(someOption).Parameters.Get("testarg").Data);

        Assert.AreEqual(cli.Front, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithSomeOptionAndMultiOptionalParameter()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        string[] args = new string[] { someOptionWithDashes, "testarg:mytest", "testarg2:mytest2", "testarg3:mytest3" };

        Map<Option>? expectedOptions = null;
        ICommandLineInterfaceFront? expectedCliFront = null;

        void action(Map<Option> options, ICommandLineInterfaceFront cliFront)
        {
            expectedOptions = options;
            expectedCliFront = cliFront;
        }

        ICommand rootCommand = Command.Create("root", "root command", action);

        ICommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

        cli.RootCommand
            .AddOption(someOption, someOptionDescription, Parameters.Create(new Parameter("testarg", 6, 10, false)
                , new("testarg2", 6, 10, false)
                , new("testarg3", 6, 10, false)));

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Itens.Count);
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?.Get(someOption).Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?.Get(someOption).Description);
        Assert.AreEqual("mytest", expectedOptions?.Get(someOption).Parameters.Get("testarg").Data);
        Assert.AreEqual("mytest2", expectedOptions?.Get(someOption).Parameters.Get("testarg2").Data);
        Assert.AreEqual("mytest3", expectedOptions?.Get(someOption).Parameters.Get("testarg3").Data);

        Assert.AreEqual(cli.Front, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithSomeOptionAndRequiredAndOptionalParameter()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        string[] args = new string[] { someOptionWithDashes, "testarg:mytest", "testreq:myreq" };

        Map<Option>? expectedOptions = null;
        ICommandLineInterfaceFront? expectedCliFront = null;

        void action(Map<Option> options, ICommandLineInterfaceFront cliFront)
        {
            expectedOptions = options;
            expectedCliFront = cliFront;
        }

        ICommand rootCommand = Command.Create("root", "root command", action);

        ICommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

        cli.RootCommand
            .AddOption(someOption, someOptionDescription, Parameters.Create(new Parameter("testarg", 6, 10, false),
                new("testreq", 5, 10)));

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Itens.Count);
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?.Get(someOption).Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?.Get(someOption).Description);
        Assert.AreEqual("mytest", expectedOptions?.Get(someOption).Parameters.Get("testarg").Data);
        Assert.AreEqual("myreq", expectedOptions?.Get(someOption).Parameters.Get("testreq").Data);

        Assert.AreEqual(cli.Front, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithSomeOptionAndMultiOptionalAndRequiredParameter()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        string[] args = new string[] { someOptionWithDashes, "testarg:mytest", "testarg2:mytest2", "testarg3:mytest3", "testarg4:mytest4", "testarg5:mytest5" };

        Map<Option>? expectedOptions = null;
        ICommandLineInterfaceFront? expectedCliFront = null;

        void action(Map<Option> options, ICommandLineInterfaceFront cliFront)
        {
            expectedOptions = options;
            expectedCliFront = cliFront;
        }

        ICommand rootCommand = Command.Create("root", "root command", action);

        ICommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

        cli.RootCommand
            .AddOption(someOption, someOptionDescription, Parameters.Create(new Parameter("testarg", 6, 10, false)
                , new("testarg2", 6, 10)
                , new("testarg3", 6, 10, false)
                , new("testarg4", 6, 10, false)
                , new("testarg5", 6, 10)));

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Itens.Count);
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?.Get(someOption).Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?.Get(someOption).Description);
        Assert.AreEqual("mytest", expectedOptions?.Get(someOption).Parameters.Get("testarg").Data);
        Assert.AreEqual("mytest2", expectedOptions?.Get(someOption).Parameters.Get("testarg2").Data);
        Assert.AreEqual("mytest3", expectedOptions?.Get(someOption).Parameters.Get("testarg3").Data);
        Assert.AreEqual("mytest4", expectedOptions?.Get(someOption).Parameters.Get("testarg4").Data);
        Assert.AreEqual("mytest5", expectedOptions?.Get(someOption).Parameters.Get("testarg5").Data);

        Assert.AreEqual(cli.Front, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithMultiOptionAndMultiOptionalAndRequiredParameter()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";
        const string someOptionWithDashes = $"--{someOption}";

        const string someOption2 = "some-option2";
        const string someOptionDescription2 = "awesome option2";
        const string someOptionWithDashes2 = $"--{someOption2}";

        string[] args = new string[] { someOptionWithDashes, "testarg:mytest", "testarg2:mytest2", someOptionWithDashes2, "testarg3:mytest3", "testarg4:mytest4", "testarg5:mytest5" };

        Map<Option>? expectedOptions = null;
        ICommandLineInterfaceFront? expectedCliFront = null;

        void action(Map<Option> options, ICommandLineInterfaceFront cliFront)
        {
            expectedOptions = options;
            expectedCliFront = cliFront;
        }

        ICommand rootCommand = Command.Create("root", "root command", action);

        ICommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

        cli.RootCommand
            .AddOption(someOption, someOptionDescription, Parameters.Create(new Parameter("testarg", 6, 10, false), new("testarg2", 6, 10)))
            .AddOption(someOption2, someOptionDescription2, Parameters.Create(new Parameter("testarg3", 6, 10, false),
                new("testarg4", 6, 10, false),
                new("testarg5", 6, 10)));

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(2, expectedOptions?.Itens.Count);

        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.IsTrue(expectedOptions?.Has(someOption2));

        Assert.AreEqual(someOption, expectedOptions?.Get(someOption).Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?.Get(someOption).Description);
        Assert.AreEqual("mytest", expectedOptions?.Get(someOption).Parameters.Get("testarg").Data);
        Assert.AreEqual("mytest2", expectedOptions?.Get(someOption).Parameters.Get("testarg2").Data);

        Assert.AreEqual(someOption2, expectedOptions?.Get(someOption2).Id);
        Assert.AreEqual(someOptionDescription2, expectedOptions?.Get(someOption2).Description);
        Assert.AreEqual("mytest3", expectedOptions?.Get(someOption2).Parameters.Get("testarg3").Data);
        Assert.AreEqual("mytest4", expectedOptions?.Get(someOption2).Parameters.Get("testarg4").Data);
        Assert.AreEqual("mytest5", expectedOptions?.Get(someOption2).Parameters.Get("testarg5").Data);

        Assert.AreEqual(cli.Front, expectedCliFront);
    }


    [Test]
    public void SuccessfulExecuteRootWithCustomCommandAndMultiOptionAndMultiOptionalAndRequiredParameter()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";
        const string someOptionWithDashes = $"--{someOption}";

        const string someOption2 = "some-option2";
        const string someOptionDescription2 = "awesome option2";
        const string someOptionWithDashes2 = $"--{someOption2}";

        string[] args = new string[] { someOptionWithDashes, "testarg:mytest", "testarg2:mytest2", "test", someOptionWithDashes2, "testarg3:mytest3", "testarg4:mytest4", "testarg5:mytest5" };

        Map<Option>? expectedOptions = null;
        ICommandLineInterfaceFront? expectedCliFront = null;

        Map<Option>? expectedOptionsCustom = null;
        ICommandLineInterfaceFront? expectedCliFrontCustom = null;

        ICommand customCommand = Command
            .Create("test", "test command description", (o, c) =>
                {
                    expectedOptionsCustom = o;
                    expectedCliFrontCustom = c;

                })
                .AddOption(someOption2, someOptionDescription2, Parameters.Create(new("testarg3", 6, 10, false),
                    new("testarg4", 6, 10, false),
                    new("testarg5", 6, 10)));

        ICommand rootCommand = Command
            .Create("root", "root command", (o, c) =>
                {
                    expectedOptions = o;
                    expectedCliFront = c;
                })
                .AddOption(someOption, someOptionDescription, Parameters.Create(new Parameter("testarg", 6, 10, false),
                        new Parameter("testarg2", 6, 10)))
                .AddCommand(customCommand);

        ICommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Itens.Count);
        Assert.AreEqual(1, expectedOptionsCustom?.Itens.Count);

        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.IsTrue(expectedOptionsCustom?.Has(someOption2));

        Assert.AreEqual(someOption, expectedOptions?.Get(someOption).Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?.Get(someOption).Description);
        Assert.AreEqual("mytest", expectedOptions?.Get(someOption).Parameters.Get("testarg").Data);
        Assert.AreEqual("mytest2", expectedOptions?.Get(someOption).Parameters.Get("testarg2").Data);

        Assert.AreEqual(someOption2, expectedOptionsCustom?.Get(someOption2).Id);
        Assert.AreEqual(someOptionDescription2, expectedOptionsCustom?.Get(someOption2).Description);
        Assert.AreEqual("mytest3", expectedOptionsCustom?.Get(someOption2).Parameters.Get("testarg3").Data);
        Assert.AreEqual("mytest4", expectedOptionsCustom?.Get(someOption2).Parameters.Get("testarg4").Data);
        Assert.AreEqual("mytest5", expectedOptionsCustom?.Get(someOption2).Parameters.Get("testarg5").Data);

        Assert.AreEqual(cli.Front, expectedCliFront);
        Assert.AreEqual(cli.Front, expectedCliFrontCustom);

        Assert.AreEqual(0, cli.RootCommand.Order);
        Assert.AreEqual(1, customCommand.Order);
    }

    [Test]
    public void SuccessfulExecuteRootWithSomeAbbrevOption()
    {

        const string someOption = "some-option";
        const string someAbbrevOption = "s";
        const string someOptionDescription = "awesome option";

        const string someAbbrevOptionWithDash = $"-{someAbbrevOption}";

        string[] args = new string[] { someAbbrevOptionWithDash };

        Map<Option>? expectedOptions = null;
        ICommandLineInterfaceFront? expectedCliFront = null;

        void action(Map<Option> options, ICommandLineInterfaceFront cliFront)
        {
            expectedOptions = options;
            expectedCliFront = cliFront;
        }

        ICommand rootCommand = Command.Create("root", "root command", action);

        ICommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

        cli.RootCommand
            .AddOption(someOption, someOptionDescription, someAbbrevOption);

        cli.Execute(args);

        Assert.NotNull(expectedOptions);
        Assert.NotNull(expectedCliFront);

        Assert.AreEqual(1, expectedOptions?.Itens.Count);
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?.Get(someOption).Id);
        Assert.AreEqual(someAbbrevOption, expectedOptions?.Get(someOption).Abbreviation);
        Assert.AreEqual(someOptionDescription, expectedOptions?.Get(someOption).Description);

        Assert.AreEqual(cli.Front, expectedCliFront);
    }

    [Test]
    public void ExecuteRootWithSomeInvalidOptionError()
    {
        const string invalidOption = "invalid-option";

        const string invalidOptionWithDashes = $"--{invalidOption}";

        string[] args = new string[] { invalidOptionWithDashes };

        ICommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

        rootCommandMock.Setup(x => x.HasOption(invalidOption)).Returns(false);

        cli.Execute(args);

        frontMock.Verify(x => x.PrintHelp(rootCommandMock.Object, It.Is<InvalidOperationException>(x => x.Message == $"The option '{invalidOptionWithDashes}' is invalid.")), Times.Once);
    }

    [Test]
    public void ExecuteRootWithSomeInvalidAbbrevOptionError()
    {
        const string invalidOption = "i";

        const string invalidOptionWithDashes = $"-{invalidOption}";

        string[] args = new string[] { invalidOptionWithDashes };

        ICommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

        rootCommandMock.Setup(x => x.HasOption(invalidOption)).Returns(false);

        cli.Execute(args);

        frontMock.Verify(x => x.PrintHelp(rootCommandMock.Object, It.Is<InvalidOperationException>(x => x.Message == $"The option '{invalidOptionWithDashes}' is invalid.")), Times.Once);
    }

    [Test]
    public void ExecuteRootWithValidOptionButNoArgsError()
    {
        const string someOption = "some-option";

        const string someOptionWithDashes = $"--{someOption}";

        string[] args = new string[] { someOptionWithDashes };

        ICommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

        rootCommandMock.Setup(x => x.HasOption(someOption)).Returns(true);
        rootCommandMock.Setup(x => x.GetOption(someOption)).Returns(new Option("some-option", "some option", Parameters.Create(new Parameter("arg1", 6, 10))));

        cli.Execute(args);

        frontMock.Verify(x => x.PrintHelp(rootCommandMock.Object, It.Is<InvalidOperationException>(x => x.Message == $"Required parameters [arg1] is missing for option: some-option")), Times.Once);
    }

    [Test]
    public void ExecuteRootWithCustomCommandAndValidOptionButNoArgsError()
    {
        const string someOption = "some-option";

        const string someOptionWithDashes = $"--{someOption}";

        string[] args = new string[] { "test", someOptionWithDashes };

        ICommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

        Mock<ICommand> customCommandMock = new();

        rootCommandMock.Setup(x => x.GetCommand("test")).Returns(customCommandMock.Object);
        rootCommandMock.Setup(x => x.HasCommand("test")).Returns(true);
        customCommandMock.Setup(x => x.HasOption(someOption)).Returns(true);
        customCommandMock.Setup(x => x.GetOption(someOption)).Returns(new Option("some-option", "some option", Parameters.Create(new Parameter("arg1", 6, 10))));

        cli.Execute(args);

        frontMock.Verify(x => x.PrintHelp(customCommandMock.Object, It.Is<InvalidOperationException>(x => x.Message == $"Required parameters [arg1] is missing for option: some-option")), Times.Once);
    }

    [Test]
    public void ExecuteRootWithHelpOption()
    {
        const string helpOption = "help";

        const string helpOptionWithDashes = $"--{helpOption}";

        string[] args = new string[] { helpOptionWithDashes };

        ICommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

        Mock<ICommand> customCommandMock = new();

        rootCommandMock.Setup(x => x.HasOption(helpOption)).Returns(true);
        rootCommandMock.Setup(x => x.GetOption(helpOption)).Returns(new Option(helpOption, "Show help on screen", "h"));

        cli.Execute(args);

        frontMock.Verify(x => x.PrintHelp(rootCommandMock.Object), Times.Once);
    }

    [Test]
    public void ExecuteRootWithArgsAndNoOptionError()
    {

        string[] args = new string[] { "testarg:mytest" };

        ICommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

        cli.Execute(args);

        frontMock.Verify(x => x.PrintHelp(rootCommandMock.Object, It.Is<InvalidOperationException>(x => x.Message == "You can't put parameters without any option that accept it.")), Times.Once);
    }

    [Test]
    public void ExecuteRootWithSomeOptionAndInvalidArgsError()
    {
        const string someOption = "some-option";

        const string someOptionWithDashes = $"--{someOption}";

        string[] args = new string[] { someOptionWithDashes, "testarg:mytest" };

        ICommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

        rootCommandMock.Setup(x => x.HasOption(someOption)).Returns(true);
        rootCommandMock.Setup(x => x.GetOption(someOption)).Returns(new Option(someOption, "some option"));

        cli.Execute(args);

        frontMock.Verify(x => x.PrintHelp(rootCommandMock.Object, It.Is<InvalidOperationException>(x => x.Message == $"The parameter 'testarg' is invalid for option: {someOption}.")), Times.Once);
    }

    [Test]
    public void ExecuteRootWithSomeOptionAndInvalidArgsByPositionError()
    {
        const string someOption = "some-option";

        const string someOptionWithDashes = $"--{someOption}";

        string[] args = new string[] { someOptionWithDashes, "testarg" };

        ICommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

        rootCommandMock.Setup(x => x.HasOption(someOption)).Returns(true);
        rootCommandMock.Setup(x => x.GetOption(someOption)).Returns(new Option(someOption, "some option"));

        cli.Execute(args);

        frontMock.Verify(x => x.PrintHelp(rootCommandMock.Object, It.Is<InvalidOperationException>(x => x.Message == $"The parameter data 'testarg' is out of bound for option: {someOption}.")), Times.Once);
    }

    [Test]
    public void ExecuteRootWithSomeOptionAndParameterByPositionAlreadyFilledError()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        string[] args = new string[] { someOptionWithDashes, "mytest", "testarg:mytest" };

        Map<Option>? expectedOptions = null;
        ICommandLineInterfaceFront? expectedCliFront = null;

        void action(Map<Option> options, ICommandLineInterfaceFront cliFront)
        {
            expectedOptions = options;
            expectedCliFront = cliFront;
        }

        ICommand rootCommand = Command.Create("root", "root command", action);

        ICommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

        cli.RootCommand
            .AddOption(someOption, someOptionDescription, Parameters.Create(new Parameter("testarg", 6, 10)));

        cli.Execute(args);

        frontMock.Verify(x => x.PrintHelp(rootCommand, It.Is<InvalidOperationException>(x => x.Message == $"The parameter 'testarg' is already filled for option: {someOption}.")), Times.Once);
    }
}