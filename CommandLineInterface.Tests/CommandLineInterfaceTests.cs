using System;
using System.Collections.Generic;
using CommandLineInterface;
using Moq;
using NUnit.Framework;

namespace CommandLineInterface.Tests;

public class CommandLineInterfaceTests
{
    private Mock<ICommandLineInterfaceFront> frontMock = new Mock<ICommandLineInterfaceFront>();
    private Mock<ICommand> rootCommandMock = new Mock<ICommand>();


    [SetUp]
    public void Setup()
    {
        frontMock.Reset();
        rootCommandMock.Reset();
    }

    [Test]
    public void SuccessfulCreateCLIAndRootCommand()
    {
        CommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);
        Assert.AreEqual(rootCommandMock.Object, cli.RootCommand);
        Assert.AreEqual(frontMock.Object, cli.Front);
    }

    [Test]
    public void SuccessfulCreateCLIAndFront()
    {
        IConsoleManager consoleManager = new ConsoleManager();
        Metadata metadata = new Metadata(title: "Test");
        CommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, consoleManager, metadata);
        Assert.AreEqual(rootCommandMock.Object, cli.RootCommand);
        Assert.AreEqual(metadata, cli.Front.Metadata);
    }

    [Test]
    public void CreateCLIWithRootAndNullFrontError()
    {
        Assert.Throws<ArgumentNullException>(() => new CommandLineInterface(rootCommandMock.Object, null));
    }

    [Test]
    public void CreateCLIWithNullRootAndFrontError()
    {
        Assert.Throws<ArgumentNullException>(() => new CommandLineInterface(null, frontMock.Object));
    }

    [Test]
    public void CreateCLIWithNullRootAndNullFrontError()
    {
        Assert.Throws<ArgumentNullException>(() => new CommandLineInterface(null, null));
    }

    [Test]
    public void SuccessfulCreateCommand()
    {
        CommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

        Action<Map<Option>, ICommandLineInterfaceFront> newAction = (x, y) => { };
        string name = "new";
        string description = "new command for test";
        ICommand command = Command.CreateCommand(name, description, newAction);

        Assert.AreEqual(name, command.Name);
        Assert.AreEqual(description, command.Description);
        Assert.AreEqual(newAction, command.Action);
    }



    [Test]
    public void SuccessfulExecuteRootWithNoArgs()
    {
        string[] args = new string[] { };

        CommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

        cli.Execute(args);

        rootCommandMock.Verify(x => x.Action, Times.Once);
    }

    [Test]
    public void SuccessfulExecuteRootWithOneEmptyArg()
    {
        string[] args = new string[] { "" };

        CommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

        cli.Execute(args);

        rootCommandMock.Verify(x => x.Action, Times.Once);
    }

    [Test]
    public void SuccessfulExecuteRootWithOneSpaceArg()
    {
        string[] args = new string[] { "  " };

        CommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

        cli.Execute(args);

        rootCommandMock.Verify(x => x.Action, Times.Once);
    }

    [Test]
    public void SuccessfulExecuteRootWithMultiSpacesArg()
    {
        string[] args = new string[] { "  ", "   ", "         ", "  " };

        CommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

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

        Action<Map<Option>, ICommandLineInterfaceFront> action = (options, cliFront) =>
        {
            expectedOptions = options;
            expectedCliFront = cliFront;
        };

        ICommand rootCommand = Command.CreateCommand("root", "root command", action);

        CommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

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
    public void SuccessfulExecuteRootWithSomeOptionAndArgument()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        string[] args = new string[] { someOptionWithDashes, "testarg:mytest" };

        Map<Option>? expectedOptions = null;
        ICommandLineInterfaceFront? expectedCliFront = null;

        Action<Map<Option>, ICommandLineInterfaceFront> action = (options, cliFront) =>
        {
            expectedOptions = options;
            expectedCliFront = cliFront;
        };

        ICommand rootCommand = Command.CreateCommand("root", "root command", action);

        CommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

        cli.RootCommand
            .AddOption(someOption, someOptionDescription, new Arguments()
                .Add("testarg"));

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Itens.Count);
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?.Get(someOption).Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?.Get(someOption).Description);
        Assert.AreEqual("mytest", expectedOptions?.Get(someOption).Arguments.Required.Get("testarg").Data);

        Assert.AreEqual(cli.Front, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithSomeOptionAndMultiArgument()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        string[] args = new string[] { someOptionWithDashes, "testarg:mytest", "testarg2:mytest2", "testarg3:mytest3" };

        Map<Option>? expectedOptions = null;
        ICommandLineInterfaceFront? expectedCliFront = null;

        Action<Map<Option>, ICommandLineInterfaceFront> action = (options, cliFront) =>
        {
            expectedOptions = options;
            expectedCliFront = cliFront;
        };

        ICommand rootCommand = Command.CreateCommand("root", "root command", action);

        CommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

        cli.RootCommand
            .AddOption(someOption, someOptionDescription, new Arguments()
                .Add("testarg")
                .Add("testarg2")
                .Add("testarg3"));

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Itens.Count);
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?.Get(someOption).Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?.Get(someOption).Description);
        Assert.AreEqual("mytest", expectedOptions?.Get(someOption).Arguments.Required.Get("testarg").Data);
        Assert.AreEqual("mytest2", expectedOptions?.Get(someOption).Arguments.Required.Get("testarg2").Data);
        Assert.AreEqual("mytest3", expectedOptions?.Get(someOption).Arguments.Required.Get("testarg3").Data);

        Assert.AreEqual(cli.Front, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithSomeOptionAndOptionalArgument()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        string[] args = new string[] { someOptionWithDashes, "testarg:mytest" };

        Map<Option>? expectedOptions = null;
        ICommandLineInterfaceFront? expectedCliFront = null;

        Action<Map<Option>, ICommandLineInterfaceFront> action = (options, cliFront) =>
        {
            expectedOptions = options;
            expectedCliFront = cliFront;
        };

        ICommand rootCommand = Command.CreateCommand("root", "root command", action);

        CommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

        cli.RootCommand
            .AddOption(someOption, someOptionDescription, new Arguments()
                .Add("testarg", false));

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Itens.Count);
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?.Get(someOption).Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?.Get(someOption).Description);
        Assert.AreEqual("mytest", expectedOptions?.Get(someOption).Arguments.Optional.Get("testarg").Data);

        Assert.AreEqual(cli.Front, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithSomeOptionAndMultiOptionalArgument()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        string[] args = new string[] { someOptionWithDashes, "testarg:mytest", "testarg2:mytest2", "testarg3:mytest3" };

        Map<Option>? expectedOptions = null;
        ICommandLineInterfaceFront? expectedCliFront = null;

        Action<Map<Option>, ICommandLineInterfaceFront> action = (options, cliFront) =>
        {
            expectedOptions = options;
            expectedCliFront = cliFront;
        };

        ICommand rootCommand = Command.CreateCommand("root", "root command", action);

        CommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

        cli.RootCommand
            .AddOption(someOption, someOptionDescription, new Arguments()
                .Add("testarg", false)
                .Add("testarg2", false)
                .Add("testarg3", false));

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Itens.Count);
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?.Get(someOption).Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?.Get(someOption).Description);
        Assert.AreEqual("mytest", expectedOptions?.Get(someOption).Arguments.Optional.Get("testarg").Data);
        Assert.AreEqual("mytest2", expectedOptions?.Get(someOption).Arguments.Optional.Get("testarg2").Data);
        Assert.AreEqual("mytest3", expectedOptions?.Get(someOption).Arguments.Optional.Get("testarg3").Data);

        Assert.AreEqual(cli.Front, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithSomeOptionAndRequiredAndOptionalArgument()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        string[] args = new string[] { someOptionWithDashes, "testarg:mytest", "testreq:myreq" };

        Map<Option>? expectedOptions = null;
        ICommandLineInterfaceFront? expectedCliFront = null;

        Action<Map<Option>, ICommandLineInterfaceFront> action = (options, cliFront) =>
        {
            expectedOptions = options;
            expectedCliFront = cliFront;
        };

        ICommand rootCommand = Command.CreateCommand("root", "root command", action);

        CommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

        cli.RootCommand
            .AddOption(someOption, someOptionDescription, new Arguments()
                .Add("testarg", false)
                .Add("testreq"));

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Itens.Count);
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?.Get(someOption).Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?.Get(someOption).Description);
        Assert.AreEqual("mytest", expectedOptions?.Get(someOption).Arguments.Optional.Get("testarg").Data);
        Assert.AreEqual("myreq", expectedOptions?.Get(someOption).Arguments.Required.Get("testreq").Data);

        Assert.AreEqual(cli.Front, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithSomeOptionAndMultiOptionalAndRequiredArgument()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        string[] args = new string[] { someOptionWithDashes, "testarg:mytest", "testarg2:mytest2", "testarg3:mytest3", "testarg4:mytest4", "testarg5:mytest5" };

        Map<Option>? expectedOptions = null;
        ICommandLineInterfaceFront? expectedCliFront = null;

        Action<Map<Option>, ICommandLineInterfaceFront> action = (options, cliFront) =>
        {
            expectedOptions = options;
            expectedCliFront = cliFront;
        };

        ICommand rootCommand = Command.CreateCommand("root", "root command", action);

        CommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

        cli.RootCommand
            .AddOption(someOption, someOptionDescription, new Arguments()
                .Add("testarg", false)
                .Add("testarg2")
                .Add("testarg3", false)
                .Add("testarg4", false)
                .Add("testarg5"));

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Itens.Count);
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?.Get(someOption).Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?.Get(someOption).Description);
        Assert.AreEqual("mytest", expectedOptions?.Get(someOption).Arguments.Optional.Get("testarg").Data);
        Assert.AreEqual("mytest2", expectedOptions?.Get(someOption).Arguments.Required.Get("testarg2").Data);
        Assert.AreEqual("mytest3", expectedOptions?.Get(someOption).Arguments.Optional.Get("testarg3").Data);
        Assert.AreEqual("mytest4", expectedOptions?.Get(someOption).Arguments.Optional.Get("testarg4").Data);
        Assert.AreEqual("mytest5", expectedOptions?.Get(someOption).Arguments.Required.Get("testarg5").Data);

        Assert.AreEqual(cli.Front, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithMultiOptionAndMultiOptionalAndRequiredArgument()
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

        Action<Map<Option>, ICommandLineInterfaceFront> action = (options, cliFront) =>
        {
            expectedOptions = options;
            expectedCliFront = cliFront;
        };

        ICommand rootCommand = Command.CreateCommand("root", "root command", action);

        CommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

        cli.RootCommand
            .AddOption(someOption, someOptionDescription, new Arguments()
                .Add("testarg", false)
                .Add("testarg2"))
            .AddOption(someOption2, someOptionDescription2, new Arguments()
                .Add("testarg3", false)
                .Add("testarg4", false)
                .Add("testarg5"));

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(2, expectedOptions?.Itens.Count);

        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.IsTrue(expectedOptions?.Has(someOption2));

        Assert.AreEqual(someOption, expectedOptions?.Get(someOption).Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?.Get(someOption).Description);
        Assert.AreEqual("mytest", expectedOptions?.Get(someOption).Arguments.Optional.Get("testarg").Data);
        Assert.AreEqual("mytest2", expectedOptions?.Get(someOption).Arguments.Required.Get("testarg2").Data);

        Assert.AreEqual(someOption2, expectedOptions?.Get(someOption2).Id);
        Assert.AreEqual(someOptionDescription2, expectedOptions?.Get(someOption2).Description);
        Assert.AreEqual("mytest3", expectedOptions?.Get(someOption2).Arguments.Optional.Get("testarg3").Data);
        Assert.AreEqual("mytest4", expectedOptions?.Get(someOption2).Arguments.Optional.Get("testarg4").Data);
        Assert.AreEqual("mytest5", expectedOptions?.Get(someOption2).Arguments.Required.Get("testarg5").Data);

        Assert.AreEqual(cli.Front, expectedCliFront);
    }


    [Test]
    public void SuccessfulExecuteRootWithCustomCommandAndMultiOptionAndMultiOptionalAndRequiredArgument()
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
            .CreateCommand("test", "test command description", (o, c) =>
                {
                    expectedOptionsCustom = o;
                    expectedCliFrontCustom = c;

                })
                .AddOption(someOption2, someOptionDescription2, new Arguments()
                    .Add("testarg3", false)
                    .Add("testarg4", false)
                    .Add("testarg5"));

        ICommand rootCommand = Command
            .CreateCommand("root", "root command", (o, c) =>
                {
                    expectedOptions = o;
                    expectedCliFront = c;
                })
                .AddOption(someOption, someOptionDescription, new Arguments()
                        .Add("testarg", false)
                        .Add("testarg2"))
                .AddCommand(customCommand);

        CommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Itens.Count);
        Assert.AreEqual(1, expectedOptionsCustom?.Itens.Count);

        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.IsTrue(expectedOptionsCustom?.Has(someOption2));

        Assert.AreEqual(someOption, expectedOptions?.Get(someOption).Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?.Get(someOption).Description);
        Assert.AreEqual("mytest", expectedOptions?.Get(someOption).Arguments.Optional.Get("testarg").Data);
        Assert.AreEqual("mytest2", expectedOptions?.Get(someOption).Arguments.Required.Get("testarg2").Data);

        Assert.AreEqual(someOption2, expectedOptionsCustom?.Get(someOption2).Id);
        Assert.AreEqual(someOptionDescription2, expectedOptionsCustom?.Get(someOption2).Description);
        Assert.AreEqual("mytest3", expectedOptionsCustom?.Get(someOption2).Arguments.Optional.Get("testarg3").Data);
        Assert.AreEqual("mytest4", expectedOptionsCustom?.Get(someOption2).Arguments.Optional.Get("testarg4").Data);
        Assert.AreEqual("mytest5", expectedOptionsCustom?.Get(someOption2).Arguments.Required.Get("testarg5").Data);

        Assert.AreEqual(cli.Front, expectedCliFront);
        Assert.AreEqual(cli.Front, expectedCliFrontCustom);

        Assert.AreEqual(0, cli.RootCommand.Order);
        Assert.AreEqual(1, customCommand.Order);
    }

    [Test]
    public void SuccessfulExecuteRootWithMultiOptionAbbrevTogether()
    {
        const string someOption = "some-option";
        const string someAbbrevOption = "a";
        const string someOptionDescription = "awesome option";
        const string someOptionWithDashes = $"-{someAbbrevOption}";

        const string someOption2 = "some-option2";
        const string someAbbrevOption2 = "b";
        const string someOptionDescription2 = "awesome option2";
        const string someOptionWithDashes2 = $"-{someAbbrevOption2}";

        string[] args = new string[] { "-ab" };

        Map<Option>? expectedOptions = null;
        ICommandLineInterfaceFront? expectedCliFront = null;

        Action<Map<Option>, ICommandLineInterfaceFront> action = (options, cliFront) =>
        {
            expectedOptions = options;
            expectedCliFront = cliFront;
        };

        ICommand rootCommand = Command.CreateCommand("root", "root command", action);

        CommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

        cli.RootCommand
            .AddOption(someOption, someOptionDescription, someAbbrevOption)
            .AddOption(someOption2, someOptionDescription2, someAbbrevOption2);

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(2, expectedOptions?.Itens.Count);

        Assert.IsTrue(expectedOptions?.Has(someOption));

        Assert.AreEqual(someOption, expectedOptions?.Get(someOption).Id);
        Assert.AreEqual(someAbbrevOption, expectedOptions?.Get(someOption).Abbreviation);
        Assert.AreEqual(someOptionDescription, expectedOptions?.Get(someOption).Description);

        Assert.AreEqual(someOption2, expectedOptions?.Get(someOption2).Id);
        Assert.AreEqual(someAbbrevOption2, expectedOptions?.Get(someOption2).Abbreviation);
        Assert.AreEqual(someOptionDescription2, expectedOptions?.Get(someOption2).Description);

        Assert.AreEqual(cli.Front, expectedCliFront);
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

        Action<Map<Option>, ICommandLineInterfaceFront> action = (options, cliFront) =>
        {
            expectedOptions = options;
            expectedCliFront = cliFront;
        };

        ICommand rootCommand = Command.CreateCommand("root", "root command", action);

        CommandLineInterface cli = new CommandLineInterface(rootCommand, frontMock.Object);

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

        CommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

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

        CommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

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

        CommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

        rootCommandMock.Setup(x => x.HasOption(someOption)).Returns(true);
        rootCommandMock.Setup(x => x.GetOption(someOption)).Returns(new Option("some-option", "some option", new Arguments().Add("arg1")));

        cli.Execute(args);

        frontMock.Verify(x => x.PrintHelp(rootCommandMock.Object, It.Is<InvalidOperationException>(x => x.Message == $"Required arguments [arg1] is missing for option: some-option")), Times.Once);
    }

    [Test]
    public void ExecuteRootWithCustomCommandAndValidOptionButNoArgsError()
    {
        const string someOption = "some-option";

        const string someOptionWithDashes = $"--{someOption}";

        string[] args = new string[] { "test", someOptionWithDashes };

        CommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

        Mock<ICommand> customCommandMock = new Mock<ICommand>();

        rootCommandMock.Setup(x => x.GetCommand("test")).Returns(customCommandMock.Object);
        customCommandMock.Setup(x => x.HasOption(someOption)).Returns(true);
        customCommandMock.Setup(x => x.GetOption(someOption)).Returns(new Option("some-option", "some option", new Arguments().Add("arg1")));

        cli.Execute(args);

        frontMock.Verify(x => x.PrintHelp(customCommandMock.Object, It.Is<InvalidOperationException>(x => x.Message == $"Required arguments [arg1] is missing for option: some-option")), Times.Once);
    }

    [Test]
    public void ExecuteRootWithHelpOption()
    {
        const string helpOption = "help";

        const string helpOptionWithDashes = $"--{helpOption}";

        string[] args = new string[] { helpOptionWithDashes };

        CommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

        Mock<ICommand> customCommandMock = new Mock<ICommand>();

        rootCommandMock.Setup(x => x.HasOption(helpOption)).Returns(true);
        rootCommandMock.Setup(x => x.GetOption(helpOption)).Returns(new Option(helpOption, "Show help on screen", "h"));

        cli.Execute(args);

        frontMock.Verify(x => x.PrintHelp(rootCommandMock.Object), Times.Once);
    }

    [Test]
    public void ExecuteRootWithArgsAndNoOptionError()
    {

        string[] args = new string[] { "testarg:mytest" };

        CommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

        cli.Execute(args);

        frontMock.Verify(x => x.PrintHelp(rootCommandMock.Object, It.Is<InvalidOperationException>(x => x.Message == "You can't put arguments without any option")), Times.Once);
    }

    [Test]
    public void ExecuteRootWithSomeOptionAndInvalidArgsError()
    {
        const string someOption = "some-option";

        const string someOptionWithDashes = $"--{someOption}";

        string[] args = new string[] { someOptionWithDashes, "testarg:mytest" };

        CommandLineInterface cli = new CommandLineInterface(rootCommandMock.Object, frontMock.Object);

        rootCommandMock.Setup(x => x.HasOption(someOption)).Returns(true);
        rootCommandMock.Setup(x => x.GetOption(someOption)).Returns(new Option(someOption, "some option"));

        cli.Execute(args);

        frontMock.Verify(x => x.PrintHelp(rootCommandMock.Object, It.Is<InvalidOperationException>(x => x.Message == $"The argument 'testarg:mytest' is invalid for option: {someOption}.")), Times.Once);
    }
}