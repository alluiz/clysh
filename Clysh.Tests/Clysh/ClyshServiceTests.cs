using System;
using System.Linq;
using Clysh.Core;
using Clysh.Helper;
using Moq;
using NUnit.Framework;

namespace Clysh.Tests;

public class ClyshServiceTests
{
    private readonly Mock<IClyshView> frontMock = new();
    private readonly Mock<IClyshCommand> rootCommandMock = new();


    [SetUp]
    public void Setup()
    {
        frontMock.Reset();
        rootCommandMock.Reset();
    }

    [Test]
    public void SuccessfulCreateCliAndRootCommand()
    {
        IClyshService cli = new ClyshService(rootCommandMock.Object, frontMock.Object);
        Assert.AreEqual(rootCommandMock.Object, cli.RootCommand);
        Assert.AreEqual(frontMock.Object, cli.View);
    }

    [Test]
    public void SuccessfulCreateCommand()
    {
        void NewAction(ClyshMap<ClyshOption> map, IClyshView clyshView) { }

        const string name = "new";
        const string description = "new command for test";

        var builder = new ClyshCommandBuilder();

        IClyshCommand command = builder
            .Id(name)
            .Description(description)
            .Action(NewAction)
            .Build();
        
        Assert.AreEqual(name, command.Id);
        Assert.AreEqual(description, command.Description);
        Assert.AreEqual(NewAction, command.Action);
    }



    [Test]
    public void SuccessfulExecuteRootWithNoArgs()
    {
        var args = Array.Empty<string>();

        IClyshService cli = new ClyshService(rootCommandMock.Object, frontMock.Object);

        cli.Execute(args);

        rootCommandMock.Verify(x => x.Action, Times.Once);
    }

    [Test]
    public void SuccessfulExecuteRootWithOneEmptyArg()
    {
        var args = new[] { "" };

        IClyshService cli = new ClyshService(rootCommandMock.Object, frontMock.Object);

        cli.Execute(args);

        rootCommandMock.Verify(x => x.Action, Times.Once);
    }

    [Test]
    public void SuccessfulExecuteRootWithOneSpaceArg()
    {
        var args = new[] { "  " };

        IClyshService cli = new ClyshService(rootCommandMock.Object, frontMock.Object);

        cli.Execute(args);

        rootCommandMock.Verify(x => x.Action, Times.Once);
    }

    [Test]
    public void SuccessfulExecuteRootWithMultiSpacesArg()
    {
        var args = new[] { "  ", "   ", "         ", "  " };

        IClyshService cli = new ClyshService(rootCommandMock.Object, frontMock.Object);

        cli.Execute(args);

        rootCommandMock.Verify(x => x.Action, Times.Once);

    }

    [Test]
    public void SuccessfulExecuteRootWithSomeOption()
    {
        const string someOption = "some-option";
        const string? someAbbrevOption = "s";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes };

        ClyshMap<ClyshOption>? expectedOptions = null;
        IClyshView? expectedCliFront = null;

        void Action(ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }
        
        var builder = new ClyshCommandBuilder();

        IClyshCommand rootCommand = builder.Id("root").Description("root command").Action(Action).Build();

        IClyshService cli = new ClyshService(rootCommand, frontMock.Object);

        var optionBuilder = new ClyshOptionBuilder();
        
        cli.RootCommand.AddOption(optionBuilder.Id(someOption).Description(someOptionDescription).Shortcut(someAbbrevOption).Build());

        cli.Execute(args);

        Assert.NotNull(expectedOptions);
        Assert.NotNull(expectedCliFront);

        Assert.AreEqual(1, expectedOptions?.Count(x => x.Value.Selected));
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?[someOption].Id);
        Assert.AreEqual(someAbbrevOption, expectedOptions?[someOption].Shortcut);
        Assert.AreEqual(someOptionDescription, expectedOptions?[someOption].Description);

        Assert.AreEqual(cli.View, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithSomeOptionAndParameter()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes, "testarg:mytest" };

        ClyshMap<ClyshOption>? expectedOptions = null;
        IClyshView? expectedCliFront = null;

        void Action(ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        IClyshCommand rootCommand = builder.Id("root").Description("root command").Action(Action).Build();

        IClyshService cli = new ClyshService(rootCommand, frontMock.Object);

        var optionBuilder = new ClyshOptionBuilder();
        
        cli.RootCommand.AddOption(optionBuilder.Id(someOption).Description(someOptionDescription).Parameters(ClyshParameters.Create(new ClyshParameter("testarg",6, 10))).Build());

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Count(x => x.Value.Selected));
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?[someOption].Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?[someOption].Description);
        Assert.AreEqual("mytest", expectedOptions?[someOption].Parameters["testarg"].Data);

        Assert.AreEqual(cli.View, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithSomeOptionAndParameterByPosition()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes, "mytest" };

        ClyshMap<ClyshOption>? expectedOptions = null;
        IClyshView? expectedCliFront = null;

        void Action(ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        IClyshCommand rootCommand = builder.Id("root").Description("root command").Action(Action).Build();

        IClyshService cli = new ClyshService(rootCommand, frontMock.Object);

        var optionBuilder = new ClyshOptionBuilder();
        
        cli.RootCommand.AddOption(optionBuilder.Id(someOption).Description(someOptionDescription).Parameters(ClyshParameters.Create(new ClyshParameter("testarg",6, 10))).Build());

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Count(x => x.Value.Selected));
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?[someOption].Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?[someOption].Description);
        Assert.AreEqual("mytest", expectedOptions?[someOption].Parameters["testarg"].Data);

        Assert.AreEqual(cli.View, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithSomeOptionAndMultiParameter()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes, "testarg:mytest", "testarg2:mytest2", "testarg3:mytest3" };

        ClyshMap<ClyshOption>? expectedOptions = null;
        IClyshView? expectedCliFront = null;

        void Action(ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        IClyshCommand rootCommand = builder.Id("root").Description("root command").Action(Action).Build();
        
        IClyshService cli = new ClyshService(rootCommand, frontMock.Object);

        var optionBuilder = new ClyshOptionBuilder();
        
        cli.RootCommand.AddOption(optionBuilder.Id(someOption).Description(someOptionDescription).Parameters(ClyshParameters.Create(new ClyshParameter("testarg",6, 10)
            , new ClyshParameter("testarg2", 6, 10)
            , new ClyshParameter("testarg3", 6, 10))).Build());

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Count(x => x.Value.Selected));
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?[someOption].Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?[someOption].Description);
        Assert.AreEqual("mytest", expectedOptions?[someOption].Parameters["testarg"].Data);
        Assert.AreEqual("mytest2", expectedOptions?[someOption].Parameters["testarg2"].Data);
        Assert.AreEqual("mytest3", expectedOptions?[someOption].Parameters["testarg3"].Data);

        Assert.AreEqual(cli.View, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithSomeOptionAndOptionalParameter()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes, "testarg:mytest" };

        ClyshMap<ClyshOption>? expectedOptions = null;
        IClyshView? expectedCliFront = null;

        void Action(ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        IClyshCommand rootCommand = builder.Id("root").Description("root command").Action(Action).Build();

        IClyshService cli = new ClyshService(rootCommand, frontMock.Object);
        
        var optionBuilder = new ClyshOptionBuilder();
        
        cli.RootCommand.AddOption(optionBuilder.Id(someOption).Description(someOptionDescription).Parameters(ClyshParameters.Create(new ClyshParameter("testarg",6, 10, false))).Build());

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Count(x => x.Value.Selected));
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?[someOption].Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?[someOption].Description);
        Assert.AreEqual("mytest", expectedOptions?[someOption].Parameters["testarg"].Data);

        Assert.AreEqual(cli.View, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithSomeOptionAndMultiOptionalParameter()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes, "testarg:mytest", "testarg2:mytest2", "testarg3:mytest3" };

        ClyshMap<ClyshOption>? expectedOptions = null;
        IClyshView? expectedCliFront = null;

        void Action(ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        IClyshCommand rootCommand = builder.Id("root").Description("root command").Action(Action).Build();

        IClyshService cli = new ClyshService(rootCommand, frontMock.Object);
        
        var optionBuilder = new ClyshOptionBuilder();
        
        cli.RootCommand.AddOption(optionBuilder.Id(someOption).Description(someOptionDescription).Parameters(ClyshParameters.Create(new ClyshParameter("testarg",6, 10, false)
            , new ClyshParameter("testarg2", 6, 10, false)
            , new ClyshParameter("testarg3",6, 10, false))).Build());

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Count(x => x.Value.Selected));
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?[someOption].Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?[someOption].Description);
        Assert.AreEqual("mytest", expectedOptions?[someOption].Parameters["testarg"].Data);
        Assert.AreEqual("mytest2", expectedOptions?[someOption].Parameters["testarg2"].Data);
        Assert.AreEqual("mytest3", expectedOptions?[someOption].Parameters["testarg3"].Data);

        Assert.AreEqual(cli.View, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithSomeOptionAndRequiredAndOptionalParameter()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes, "testarg:mytest", "testreq:myreq" };

        ClyshMap<ClyshOption>? expectedOptions = null;
        IClyshView? expectedCliFront = null;

        void Action(ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        IClyshCommand rootCommand = builder.Id("root").Description("root command").Action(Action).Build();

        IClyshService cli = new ClyshService(rootCommand, frontMock.Object);
        
        var optionBuilder = new ClyshOptionBuilder();
        
        cli.RootCommand.AddOption(optionBuilder
            .Id(someOption)
            .Description(someOptionDescription)
            .Parameters(ClyshParameters.Create(
                new ClyshParameter("testarg",6, 10, false),
                new ClyshParameter("testreq", 5, 10))).Build());

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Count(x => x.Value.Selected));
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?[someOption].Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?[someOption].Description);
        Assert.AreEqual("mytest", expectedOptions?[someOption].Parameters["testarg"].Data);
        Assert.AreEqual("myreq", expectedOptions?[someOption].Parameters["testreq"].Data);

        Assert.AreEqual(cli.View, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithSomeOptionAndMultiOptionalAndRequiredParameter()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes, "testarg:mytest", "testarg2:mytest2", "testarg3:mytest3", "testarg4:mytest4", "testarg5:mytest5" };

        ClyshMap<ClyshOption>? expectedOptions = null;
        IClyshView? expectedCliFront = null;

        void Action(ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        IClyshCommand rootCommand = builder.Id("root").Description("root command").Action(Action).Build();

        IClyshService cli = new ClyshService(rootCommand, frontMock.Object);
        
        var optionBuilder = new ClyshOptionBuilder();
        
        cli.RootCommand.AddOption(optionBuilder.Id(someOption).Description(someOptionDescription).Parameters(ClyshParameters.Create(new ClyshParameter("testarg",6, 10, false)
            , new ClyshParameter("testarg2", 6, 10)
            , new ClyshParameter("testarg3", 6, 10, false)
            , new ClyshParameter("testarg4", 6, 10, false)
            , new ClyshParameter("testarg5", 6, 10))).Build());

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Count(x => x.Value.Selected));
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?[someOption].Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?[someOption].Description);
        Assert.AreEqual("mytest", expectedOptions?[someOption].Parameters["testarg"].Data);
        Assert.AreEqual("mytest2", expectedOptions?[someOption].Parameters["testarg2"].Data);
        Assert.AreEqual("mytest3", expectedOptions?[someOption].Parameters["testarg3"].Data);
        Assert.AreEqual("mytest4", expectedOptions?[someOption].Parameters["testarg4"].Data);
        Assert.AreEqual("mytest5", expectedOptions?[someOption].Parameters["testarg5"].Data);

        Assert.AreEqual(cli.View, expectedCliFront);
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

        var args = new[] { someOptionWithDashes, "testarg:mytest", "testarg2:mytest2", someOptionWithDashes2, "testarg3:mytest3", "testarg4:mytest4", "testarg5:mytest5" };

        ClyshMap<ClyshOption>? expectedOptions = null;
        IClyshView? expectedCliFront = null;

        void Action(ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        IClyshCommand rootCommand = builder.Id("root").Description("root command").Action(Action).Build();

        IClyshService cli = new ClyshService(rootCommand, frontMock.Object);
        
        var optionBuilder = new ClyshOptionBuilder();
        
        cli.RootCommand.AddOption(optionBuilder.Id(someOption).Description(someOptionDescription).Parameters(ClyshParameters.Create(new ClyshParameter("testarg",6, 10, false), new ClyshParameter("testarg2", 6, 10))).Build());
        cli.RootCommand.AddOption(optionBuilder.Id(someOption2).Description(someOptionDescription2).Parameters(ClyshParameters.Create(new ClyshParameter("testarg3",6, 10, false),
            new ClyshParameter("testarg4", 6, 10, false),
            new ClyshParameter("testarg5", 6, 10))).Build());

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(2, expectedOptions?.Count(x => x.Value.Selected));

        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.IsTrue(expectedOptions?.Has(someOption2));

        Assert.AreEqual(someOption, expectedOptions?[someOption].Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?[someOption].Description);
        Assert.AreEqual("mytest", expectedOptions?[someOption].Parameters["testarg"].Data);
        Assert.AreEqual("mytest2", expectedOptions?[someOption].Parameters["testarg2"].Data);

        Assert.AreEqual(someOption2, expectedOptions?[someOption2].Id);
        Assert.AreEqual(someOptionDescription2, expectedOptions?[someOption2].Description);
        Assert.AreEqual("mytest3", expectedOptions?[someOption2].Parameters["testarg3"].Data);
        Assert.AreEqual("mytest4", expectedOptions?[someOption2].Parameters["testarg4"].Data);
        Assert.AreEqual("mytest5", expectedOptions?[someOption2].Parameters["testarg5"].Data);

        Assert.AreEqual(cli.View, expectedCliFront);
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

        var args = new[] { someOptionWithDashes, "testarg:mytest", "testarg2:mytest2", "test", someOptionWithDashes2, "testarg3:mytest3", "testarg4:mytest4", "testarg5:mytest5" };

        ClyshMap<ClyshOption>? expectedOptions = null;
        IClyshView? expectedCliFront = null;

        ClyshMap<ClyshOption>? expectedOptionsCustom = null;
        IClyshView? expectedCliFrontCustom = null;

        var builder = new ClyshCommandBuilder();
        var optionBuilder = new ClyshOptionBuilder();

        var customCommand = builder
            .Id("test")
            .Description("test command description")
            .Action((o, c) =>
            {
                expectedOptionsCustom = o;
                expectedCliFrontCustom = c;

            })
            .Option(optionBuilder
                .Id(someOption2)
                .Description(someOptionDescription2)
                .Parameters(ClyshParameters.Create(new ClyshParameter("testarg3", 6, 10, false),
                    new ClyshParameter("testarg4", 6, 10, false),
                    new ClyshParameter("testarg5", 6, 10)))
                .Build())
            .Build();
        
        var rootCommand = builder
            .Id("root")
            .Description("root command")
            .Action((o, c) =>
            {
                expectedOptions = o;
                expectedCliFront = c;
            })
            .Option(optionBuilder
                .Id(someOption)
                .Description(someOptionDescription)
                .Parameters(ClyshParameters.Create(new ClyshParameter("testarg",6, 10, false),
                    new ClyshParameter("testarg2",6, 10)))
                .Build())
            .Child(customCommand)
            .Build();

        IClyshService cli = new ClyshService(rootCommand, frontMock.Object);

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Count(x => x.Value.Selected));
        Assert.AreEqual(1, expectedOptionsCustom?.Count(x => x.Value.Selected));

        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.IsTrue(expectedOptionsCustom?.Has(someOption2));

        Assert.AreEqual(someOption, expectedOptions?[someOption].Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?[someOption].Description);
        Assert.AreEqual("mytest", expectedOptions?[someOption].Parameters["testarg"].Data);
        Assert.AreEqual("mytest2", expectedOptions?[someOption].Parameters["testarg2"].Data);

        Assert.AreEqual(someOption2, expectedOptionsCustom?[someOption2].Id);
        Assert.AreEqual(someOptionDescription2, expectedOptionsCustom?[someOption2].Description);
        Assert.AreEqual("mytest3", expectedOptionsCustom?[someOption2].Parameters["testarg3"].Data);
        Assert.AreEqual("mytest4", expectedOptionsCustom?[someOption2].Parameters["testarg4"].Data);
        Assert.AreEqual("mytest5", expectedOptionsCustom?[someOption2].Parameters["testarg5"].Data);

        Assert.AreEqual(cli.View, expectedCliFront);
        Assert.AreEqual(cli.View, expectedCliFrontCustom);

        Assert.AreEqual(0, cli.RootCommand.Order);
        Assert.AreEqual(1, customCommand.Order);
    }

    [Test]
    public void SuccessfulExecuteRootWithSomeAbbrevOption()
    {

        const string someOption = "some-option";
        const string? someAbbrevOption = "s";
        const string someOptionDescription = "awesome option";

        const string someAbbrevOptionWithDash = $"-{someAbbrevOption}";

        var args = new[] { someAbbrevOptionWithDash };

        ClyshMap<ClyshOption>? expectedOptions = null;
        IClyshView? expectedCliFront = null;

        void Action(ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        IClyshCommand rootCommand = builder.Id("root").Description("root command").Action(Action).Build();

        IClyshService cli = new ClyshService(rootCommand, frontMock.Object);

        var optionBuilder = new ClyshOptionBuilder();
        
        cli.RootCommand.AddOption(optionBuilder.Id(someOption).Description(someOptionDescription).Shortcut(someAbbrevOption).Build());

        cli.Execute(args);

        Assert.NotNull(expectedOptions);
        Assert.NotNull(expectedCliFront);

        Assert.AreEqual(1, expectedOptions?.Count(x => x.Value.Selected));
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?[someOption].Id);
        Assert.AreEqual(someAbbrevOption, expectedOptions?[someOption].Shortcut);
        Assert.AreEqual(someOptionDescription, expectedOptions?[someOption].Description);

        Assert.AreEqual(cli.View, expectedCliFront);
    }

    [Test]
    public void ExecuteRootWithSomeInvalidOptionError()
    {
        const string invalidOption = "invalid-option";

        const string invalidOptionWithDashes = $"--{invalidOption}";

        var args = new[] { invalidOptionWithDashes };

        IClyshService cli = new ClyshService(rootCommandMock.Object, frontMock.Object);

        rootCommandMock.Setup(x => x.HasOption(invalidOption)).Returns(false);

        cli.Execute(args);

        frontMock.Verify(x => x.PrintHelp(rootCommandMock.Object, It.Is<InvalidOperationException>(y => y.Message == $"The option '{invalidOptionWithDashes}' is invalid.")), Times.Once);
    }

    [Test]
    public void ExecuteRootWithSomeInvalidAbbrevOptionError()
    {
        const string invalidOption = "i";

        const string invalidOptionWithDashes = $"-{invalidOption}";

        var args = new[] { invalidOptionWithDashes };

        IClyshService cli = new ClyshService(rootCommandMock.Object, frontMock.Object);

        rootCommandMock.Setup(x => x.HasOption(invalidOption)).Returns(false);

        cli.Execute(args);

        frontMock.Verify(x => x.PrintHelp(rootCommandMock.Object, It.Is<InvalidOperationException>(y => y.Message == $"The option '{invalidOptionWithDashes}' is invalid.")), Times.Once);
    }

    [Test]
    public void ExecuteRootWithValidOptionButNoArgsError()
    {
        const string someOption = "some-option";

        const string someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes };

        IClyshService cli = new ClyshService(rootCommandMock.Object, frontMock.Object);

        var optionBuilder = new ClyshOptionBuilder();
        
        rootCommandMock.Setup(x => x.HasOption(someOption)).Returns(true);
        rootCommandMock.Setup(x => x.GetOption(someOption)).Returns(optionBuilder
            .Id("some-option")
            .Description("some option")
            .Parameters(ClyshParameters.Create(new ClyshParameter("testarg",6, 10)))
            .Build());

        cli.Execute(args);

        frontMock.Verify(x => x.PrintHelp(rootCommandMock.Object, It.Is<InvalidOperationException>(y => y.Message == $"Required parameters [testarg] is missing for option: some-option (shortcut: <null>)")), Times.Once);
    }

    [Test]
    public void ExecuteRootWithHelpOption()
    {
        const string helpOption = "help";

        const string helpOptionWithDashes = $"--{helpOption}";

        var args = new[] { helpOptionWithDashes };

        IClyshService cli = new ClyshService(rootCommandMock.Object, frontMock.Object);

        rootCommandMock.Setup(x => x.HasOption(helpOption)).Returns(true);
        
        var optionBuilder = new ClyshOptionBuilder();
        
        rootCommandMock.Setup(x => x.GetOption(helpOption)).Returns(optionBuilder
            .Id(helpOption)
            .Description("Show help on screen")
            .Shortcut("h")
            .Build());

        cli.Execute(args);

        frontMock.Verify(x => x.PrintHelp(rootCommandMock.Object), Times.Once);
    }

    [Test]
    public void ExecuteRootWithArgsAndNoOptionError()
    {

        var args = new[] { "testarg:mytest" };

        IClyshService cli = new ClyshService(rootCommandMock.Object, frontMock.Object);

        cli.Execute(args);

        frontMock.Verify(x => x.PrintHelp(rootCommandMock.Object, It.Is<InvalidOperationException>(y => y.Message == "You can't put parameters without any option that accept it.")), Times.Once);
    }

    [Test]
    public void ExecuteRootWithSomeOptionAndInvalidArgsError()
    {
        const string someOption = "some-option";

        const string someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes, "testarg:mytest" };

        IClyshService cli = new ClyshService(rootCommandMock.Object, frontMock.Object);

        rootCommandMock.Setup(x => x.HasOption(someOption)).Returns(true);
        
        var optionBuilder = new ClyshOptionBuilder();
        
        rootCommandMock.Setup(x => x.GetOption(someOption)).Returns(optionBuilder
            .Id(someOption)
            .Description("some option")
            .Build());

        cli.Execute(args);

        frontMock.Verify(x => x.PrintHelp(rootCommandMock.Object, It.Is<InvalidOperationException>(y => y.Message == $"The parameter 'testarg' is invalid for option: {someOption}.")), Times.Once);
    }

    [Test]
    public void ExecuteRootWithSomeOptionAndInvalidArgsByPositionError()
    {
        const string someOption = "some-option";

        const string someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes, "testarg" };

        IClyshService cli = new ClyshService(rootCommandMock.Object, frontMock.Object);

        rootCommandMock.Setup(x => x.HasOption(someOption)).Returns(true);
        
        var optionBuilder = new ClyshOptionBuilder();
        
        rootCommandMock.Setup(x => x.GetOption(someOption))
            .Returns(
                optionBuilder
                .Id(someOption)
                .Description("some option")
                .Build());

        cli.Execute(args);

        frontMock.Verify(x => x.PrintHelp(rootCommandMock.Object, It.Is<InvalidOperationException>(y => y.Message == $"The parameter data 'testarg' is out of bound for option: {someOption}.")), Times.Once);
    }

    [Test]
    public void ExecuteRootWithSomeOptionAndParameterByPositionAlreadyFilledError()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes, "mytest", "testarg:mytest" };

        void Action(ClyshMap<ClyshOption> options, IClyshView view)
        {
        }

        var builder = new ClyshCommandBuilder();

        IClyshCommand rootCommand = builder.Id("root").Description("root command").Action(Action).Build();

        IClyshService cli = new ClyshService(rootCommand, frontMock.Object);

        var optionBuilder = new ClyshOptionBuilder();
        
        cli.RootCommand.AddOption(optionBuilder
            .Id(someOption)
            .Description(someOptionDescription)
            .Parameters(ClyshParameters.Create(new ClyshParameter("testarg",6, 10))).Build());

        cli.Execute(args);

        frontMock.Verify(x => x.PrintHelp(rootCommand, It.Is<InvalidOperationException>(y => y.Message == $"The parameter 'testarg' is already filled for option: {someOption}.")), Times.Once);
    }
}