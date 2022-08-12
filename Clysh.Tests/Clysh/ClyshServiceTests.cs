using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Clysh.Core;
using Clysh.Core.Builder;
using Clysh.Helper;
using Moq;
using NUnit.Framework;

namespace Clysh.Tests;

public class ClyshServiceTests
{
    private readonly ClyshParameterBuilder parameterBuilder = new();
    private readonly Mock<IClyshCommand> rootCommandMock = new();
    private readonly Mock<IClyshView> viewMock = new();


    [SetUp]
    public void Setup()
    {
        viewMock.Reset();
        rootCommandMock.Reset();
    }

    [Test]
    public void SuccessfulCreateCliAndRootCommand()
    {
        IClyshService cli = new ClyshService(rootCommandMock.Object, viewMock.Object);
        Assert.AreEqual(rootCommandMock.Object, cli.RootCommand);
        Assert.AreEqual(viewMock.Object, cli.View);
    }

    [Test]
    public void SuccessfulCreateCommand()
    {
        const string name = "new";
        const string description = "new command for test";

        var builder = new ClyshCommandBuilder();

        IClyshCommand command = builder
            .Id(name)
            .Description(description)
            .Action(EmptyAction)
            .Build();

        Assert.AreEqual(name, command.Id);
        Assert.AreEqual(description, command.Description);
        Assert.AreEqual((Action<IClyshCommand, ClyshMap<ClyshOption>, IClyshView>)EmptyAction, command.Action);
    }

    private void EmptyAction(IClyshCommand clyshCommand, ClyshMap<ClyshOption> map, IClyshView clyshView)
    {
        //Do nothing. This action is just to bind with command for test
    }


    [Test]
    public void SuccessfulExecuteRootWithNoArgs()
    {
        var args = Array.Empty<string>();

        IClyshService cli = new ClyshService(rootCommandMock.Object, viewMock.Object);

        rootCommandMock.Setup(x => x.SubCommands).Returns(new ClyshMap<IClyshCommand>());
        rootCommandMock.Setup(x => x.Options).Returns(new ClyshMap<ClyshOption>());

        cli.Execute(args);

        rootCommandMock.Verify(x => x.Action, Times.Exactly(2));
    }

    [Test]
    public void SuccessfulExecuteRootWithOneEmptyArg()
    {
        var args = new[] { "" };

        IClyshService cli = new ClyshService(rootCommandMock.Object, viewMock.Object);

        rootCommandMock.Setup(x => x.SubCommands).Returns(new ClyshMap<IClyshCommand>());
        rootCommandMock.Setup(x => x.Options).Returns(new ClyshMap<ClyshOption>());

        cli.Execute(args);

        rootCommandMock.Verify(x => x.Action, Times.Exactly(2));
    }

    [Test]
    public void SuccessfulExecuteRootWithOneSpaceArg()
    {
        var args = new[] { "  " };

        IClyshService cli = new ClyshService(rootCommandMock.Object, viewMock.Object);

        rootCommandMock.Setup(x => x.SubCommands).Returns(new ClyshMap<IClyshCommand>());
        rootCommandMock.Setup(x => x.Options).Returns(new ClyshMap<ClyshOption>());

        cli.Execute(args);

        rootCommandMock.Verify(x => x.Action, Times.Exactly(2));
    }

    [Test]
    public void SuccessfulExecuteRootWithMultiSpacesArg()
    {
        var args = new[] { "  ", "   ", "         ", "  " };

        IClyshService cli = new ClyshService(rootCommandMock.Object, viewMock.Object);

        rootCommandMock.Setup(x => x.SubCommands).Returns(new ClyshMap<IClyshCommand>());
        rootCommandMock.Setup(x => x.Options).Returns(new ClyshMap<ClyshOption>());

        cli.Execute(args);

        rootCommandMock.Verify(x => x.Action, Times.Exactly(2));
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

        void Action(IClyshCommand command, ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        IClyshCommand rootCommand = builder.Id("root").Description("root command").Action(Action).Build();

        IClyshService cli = new ClyshService(rootCommand, viewMock.Object);

        var optionBuilder = new ClyshOptionBuilder();

        cli.RootCommand.AddOption(optionBuilder.Id(someOption, someAbbrevOption).Description(someOptionDescription)
            .Build());

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

        void Action(IClyshCommand command, ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        IClyshCommand rootCommand = builder.Id("root").Description("root command").Action(Action).Build();

        IClyshService cli = new ClyshService(rootCommand, viewMock.Object);

        var optionBuilder = new ClyshOptionBuilder();

        cli.RootCommand.AddOption(optionBuilder
            .Id(someOption)
            .Description(someOptionDescription)
            .Parameter(parameterBuilder
                .Id("testarg")
                .Range(6, 10)
                .Required(true)
                .Build())
            .Build());

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

        void Action(IClyshCommand command, ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        IClyshCommand rootCommand = builder.Id("root").Description("root command").Action(Action).Build();

        IClyshService cli = new ClyshService(rootCommand, viewMock.Object);

        var optionBuilder = new ClyshOptionBuilder();

        cli.RootCommand.AddOption(optionBuilder.Id(someOption).Description(someOptionDescription)
            .Parameter(parameterBuilder.Id("testarg").Range(6, 10).Required(true).Build())
            .Build());

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

        void Action(IClyshCommand command, ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        IClyshCommand rootCommand = builder.Id("root").Description("root command").Action(Action).Build();

        IClyshService cli = new ClyshService(rootCommand, viewMock.Object);

        var optionBuilder = new ClyshOptionBuilder();

        cli.RootCommand.AddOption(optionBuilder
            .Id(someOption)
            .Description(someOptionDescription)
            .Parameter(parameterBuilder.Id("testarg").Range(6, 10).Order(0).Required(true).Build())
            .Parameter(parameterBuilder.Id("testarg2").Range(6, 10).Order(1).Required(true).Build())
            .Parameter(parameterBuilder.Id("testarg3").Range(6, 10).Order(2).Required(true).Build())
            .Build());

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

        void Action(IClyshCommand command, ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        IClyshCommand rootCommand = builder.Id("root").Description("root command").Action(Action).Build();

        IClyshService cli = new ClyshService(rootCommand, viewMock.Object);

        var optionBuilder = new ClyshOptionBuilder();

        cli.RootCommand.AddOption(optionBuilder.Id(someOption).Description(someOptionDescription)
            .Parameter(parameterBuilder.Id("testarg").Range(6, 10).Required(false).Build())
            .Build());

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

        void Action(IClyshCommand command, ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        IClyshCommand rootCommand = builder.Id("root").Description("root command").Action(Action).Build();

        IClyshService cli = new ClyshService(rootCommand, viewMock.Object);

        var optionBuilder = new ClyshOptionBuilder();

        cli.RootCommand.AddOption(optionBuilder.Id(someOption).Description(someOptionDescription)
            .Parameter(parameterBuilder.Id("testarg2").Range(6, 10).Order(0).Required(false).Build())
            .Parameter(parameterBuilder.Id("testarg3").Range(6, 10).Order(1).Required(false).Build())
            .Parameter(parameterBuilder.Id("testarg").Range(6, 10).Order(2).Required(false).Build())
            .Build());

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

        var args = new[] { someOptionWithDashes, "testarg:mytest", "testarg2:mytest2" };

        ClyshMap<ClyshOption>? expectedOptions = null;
        IClyshView? expectedCliFront = null;

        void Action(IClyshCommand command, ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        IClyshCommand rootCommand = builder.Id("root").Description("root command").Action(Action).Build();

        IClyshService cli = new ClyshService(rootCommand, viewMock.Object);

        var optionBuilder = new ClyshOptionBuilder();

        cli.RootCommand.AddOption(optionBuilder
            .Id(someOption)
            .Description(someOptionDescription)
            .Parameter(parameterBuilder.Id("testarg2").Range(6, 10).Order(0).Required(true).Build())
            .Parameter(parameterBuilder.Id("testarg").Range(6, 10).Order(1).Required(false).Build())
            .Build());

        cli.Execute(args);

        Assert.NotNull(expectedCliFront);
        Assert.NotNull(expectedOptions);

        Assert.AreEqual(1, expectedOptions?.Count(x => x.Value.Selected));
        Assert.IsTrue(expectedOptions?.Has(someOption));
        Assert.AreEqual(someOption, expectedOptions?[someOption].Id);
        Assert.AreEqual(someOptionDescription, expectedOptions?[someOption].Description);
        Assert.AreEqual("mytest", expectedOptions?[someOption].Parameters["testarg"].Data);
        Assert.AreEqual("mytest2", expectedOptions?[someOption].Parameters["testarg2"].Data);

        Assert.AreEqual(cli.View, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithSomeOptionAndMultiOptionalAndRequiredParameter()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        var args = new[]
        {
            someOptionWithDashes, "testarg:mytest", "testarg2:mytest2", "testarg3:mytest3", "testarg4:mytest4",
            "testarg5:mytest5"
        };

        ClyshMap<ClyshOption>? expectedOptions = null;
        IClyshView? expectedCliFront = null;

        void Action(IClyshCommand command, ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        IClyshCommand rootCommand = builder.Id("root").Description("root command").Action(Action).Build();

        IClyshService cli = new ClyshService(rootCommand, viewMock.Object);

        var optionBuilder = new ClyshOptionBuilder();

        cli.RootCommand.AddOption(optionBuilder.Id(someOption).Description(someOptionDescription)
            .Parameter(parameterBuilder.Id("testarg5").Range(6, 10).Order(0).Required(true).Build())
            .Parameter(parameterBuilder.Id("testarg2").Range(6, 10).Order(1).Required(true).Build())
            .Parameter(parameterBuilder.Id("testarg").Range(6, 10).Order(2).Required(false).Build())
            .Parameter(parameterBuilder.Id("testarg3").Range(6, 10).Order(3).Required(false).Build())
            .Parameter(parameterBuilder.Id("testarg4").Range(6, 10).Order(4).Required(false).Build())
            .Build());

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

        var args = new[]
        {
            someOptionWithDashes, "testarg:mytest", "testarg2:mytest2", someOptionWithDashes2, "testarg3:mytest3",
            "testarg4:mytest4", "testarg5:mytest5"
        };

        ClyshMap<ClyshOption>? expectedOptions = null;
        IClyshView? expectedCliFront = null;

        void Action(IClyshCommand command, ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        IClyshCommand rootCommand = builder.Id("root").Description("root command").Action(Action).Build();

        IClyshService cli = new ClyshService(rootCommand, viewMock.Object);

        var optionBuilder = new ClyshOptionBuilder();

        cli.RootCommand.AddOption(optionBuilder.Id(someOption).Description(someOptionDescription)
            .Parameter(parameterBuilder.Id("testarg2").Range(6, 10).Order(1).Required(true).Build())
            .Parameter(parameterBuilder.Id("testarg").Range(6, 10).Order(2).Required(false).Build())
            .Build());

        cli.RootCommand.AddOption(optionBuilder.Id(someOption2).Description(someOptionDescription2)
            .Parameter(parameterBuilder.Id("testarg5").Range(6, 10).Order(0).Required(true).Build())
            .Parameter(parameterBuilder.Id("testarg3").Range(6, 10).Order(1).Required(false).Build())
            .Parameter(parameterBuilder.Id("testarg4").Range(6, 10).Order(2).Required(false).Build())
            .Build());

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

        var args = new[]
        {
            someOptionWithDashes, "testarg:mytest", "testarg2:mytest2", "test", someOptionWithDashes2,
            "testarg3:mytest3", "testarg4:mytest4", "testarg5:mytest5"
        };

        ClyshMap<ClyshOption>? expectedOptions = null;
        IClyshView? expectedCliFront = null;

        ClyshMap<ClyshOption>? expectedOptionsCustom = null;
        IClyshView? expectedCliFrontCustom = null;

        var builder = new ClyshCommandBuilder();
        var optionBuilder = new ClyshOptionBuilder();

        var customCommand = builder
            .Id("root.test")
            .Description("test command description")
            .Action((_, o, c) =>
            {
                expectedOptionsCustom = o;
                expectedCliFrontCustom = c;
            })
            .Option(optionBuilder
                .Id(someOption2)
                .Description(someOptionDescription2)
                .Parameter(parameterBuilder.Id("testarg5").Range(6, 10).Required(true).Order(0).Build())
                .Parameter(parameterBuilder.Id("testarg3").Range(6, 10).Required(false).Order(1).Build())
                .Parameter(parameterBuilder.Id("testarg4").Range(6, 10).Required(false).Order(2).Build())
                .Build())
            .Build();

        var rootCommand = builder
            .Id("root")
            .Description("root command")
            .Action((_, o, c) =>
            {
                expectedOptions = o;
                expectedCliFront = c;
            })
            .Option(optionBuilder
                .Id(someOption)
                .Description(someOptionDescription)
                .Parameter(parameterBuilder.Id("testarg2").Range(6, 10).Order(0).Required(true).Build())
                .Parameter(parameterBuilder.Id("testarg").Range(6, 10).Order(1).Required(false).Build())
                .Build())
            .SubCommand(customCommand)
            .Build();

        IClyshService cli = new ClyshService(rootCommand, viewMock.Object);

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

        void Action(IClyshCommand command, ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        IClyshCommand rootCommand = builder.Id("root").Description("root command").Action(Action).Build();

        IClyshService cli = new ClyshService(rootCommand, viewMock.Object);

        var optionBuilder = new ClyshOptionBuilder();

        cli.RootCommand.AddOption(optionBuilder.Id(someOption, someAbbrevOption).Description(someOptionDescription)
            .Build());

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

        IClyshService cli = new ClyshService(rootCommandMock.Object, viewMock.Object);

        rootCommandMock.Setup(x => x.HasOption(invalidOption)).Returns(false);
        rootCommandMock.Setup(x => x.SubCommands).Returns(new ClyshMap<IClyshCommand>());
        rootCommandMock.Setup(x => x.Options).Returns(new ClyshMap<ClyshOption>());

        cli.Execute(args);

        viewMock.Verify(
            x => x.PrintException(
                It.Is<ValidationException>(
                    y => y.Message == $"The option '{invalidOptionWithDashes}' is invalid.")), Times.Once);
    }

    [Test]
    public void ExecuteRootWithSomeInvalidAbbrevOptionError()
    {
        const string invalidOption = "i";

        const string invalidOptionWithDashes = $"-{invalidOption}";

        var args = new[] { invalidOptionWithDashes };

        IClyshService cli = new ClyshService(rootCommandMock.Object, viewMock.Object);

        rootCommandMock.Setup(x => x.HasOption(invalidOption)).Returns(false);
        rootCommandMock.Setup(x => x.SubCommands).Returns(new ClyshMap<IClyshCommand>());
        rootCommandMock.Setup(x => x.Options).Returns(new ClyshMap<ClyshOption>());

        cli.Execute(args);

        viewMock.Verify(
            x => x.PrintException(
                It.Is<ValidationException>(
                    y => y.Message == $"The option '{invalidOptionWithDashes}' is invalid.")), Times.Once);
    }

    [Test]
    public void ExecuteRootWithValidOptionButNoArgsError()
    {
        const string someOption = "some-option";

        const string someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes };

        IClyshService cli = new ClyshService(rootCommandMock.Object, viewMock.Object);

        var optionBuilder = new ClyshOptionBuilder();

        rootCommandMock.Setup(x => x.SubCommands).Returns(new ClyshMap<IClyshCommand>());
        rootCommandMock.Setup(x => x.HasOption(someOption)).Returns(true);
        var option = optionBuilder
            .Id(someOption)
            .Description("some option")
            .Parameter(parameterBuilder.Id("testarg").Range(6, 10).Required(true).Build())
            .Build();
        rootCommandMock.Setup(x => x.GetOption(someOption)).Returns(option);
        rootCommandMock.Setup(x => x.Options).Returns(new ClyshMap<ClyshOption>() { { someOption, option } });

        cli.Execute(args);

        viewMock.Verify(
            x => x.PrintException(
                It.Is<ValidationException>(y =>
                    y.Message ==
                    $"Required parameters [testarg] is missing for option: some-option (shortcut: <null>)")),
            Times.Once);
    }

    [Test]
    public void ExecuteRootWithHelpOption()
    {
        const string helpOption = "help";

        const string helpOptionWithDashes = $"--{helpOption}";

        var args = new[] { helpOptionWithDashes };

        IClyshService cli = new ClyshService(rootCommandMock.Object, viewMock.Object);

        rootCommandMock.Setup(x => x.HasOption(helpOption)).Returns(true);
        rootCommandMock.Setup(x => x.SubCommands).Returns(new ClyshMap<IClyshCommand>());

        var optionBuilder = new ClyshOptionBuilder();

        var option = optionBuilder
            .Id(helpOption, "h")
            .Description("Show help on screen")
            .Build();

        rootCommandMock.Setup(x => x.GetOption(helpOption)).Returns(option);
        rootCommandMock.Setup(x => x.Options).Returns(new ClyshMap<ClyshOption>() { { helpOption, option } });

        cli.Execute(args);

        viewMock.Verify(x => x.PrintHelp(rootCommandMock.Object), Times.Once);
    }

    [Test]
    public void ExecuteRootWithArgsAndNoOptionError()
    {
        var args = new[] { "testarg:mytest" };

        rootCommandMock.Setup(x => x.Options).Returns(new ClyshMap<ClyshOption>());
        rootCommandMock.Setup(x => x.SubCommands).Returns(new ClyshMap<IClyshCommand>());

        IClyshService cli = new ClyshService(rootCommandMock.Object, viewMock.Object);

        cli.Execute(args);

        viewMock.Verify(
            x => x.PrintException(
                It.Is<ValidationException>(y =>
                    y.Message == "You can't put parameters without any option that accept it 'testarg:mytest'")), Times.Once);
    }

    [Test]
    public void ExecuteRootWithSomeOptionAndInvalidArgsError()
    {
        const string someOption = "some-option";

        const string someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes, "testarg:mytest" };

        IClyshService cli = new ClyshService(rootCommandMock.Object, viewMock.Object);

        rootCommandMock.Setup(x => x.HasOption(someOption)).Returns(true);
        rootCommandMock.Setup(x => x.SubCommands).Returns(new ClyshMap<IClyshCommand>());

        var optionBuilder = new ClyshOptionBuilder();

        var option = optionBuilder
            .Id(someOption)
            .Description("some option")
            .Build();
        rootCommandMock.Setup(x => x.GetOption(someOption)).Returns(option);

        rootCommandMock.Setup(x => x.Options).Returns(new ClyshMap<ClyshOption>() { { someOption, option } });

        cli.Execute(args);

        viewMock.Verify(
            x => x.PrintException(
                It.Is<ValidationException>(y =>
                    y.Message == $"The parameter 'testarg' is invalid for option: {someOption}.")), Times.Once);
    }

    [Test]
    public void ExecuteRootWithSomeOptionAndInvalidArgsByPositionError()
    {
        const string someOption = "some-option";

        const string someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes, "testarg" };

        IClyshService cli = new ClyshService(rootCommandMock.Object, viewMock.Object);

        rootCommandMock.Setup(x => x.HasOption(someOption)).Returns(true);
        rootCommandMock.Setup(x => x.SubCommands).Returns(new ClyshMap<IClyshCommand>());

        var optionBuilder = new ClyshOptionBuilder();

        var option = optionBuilder
            .Id(someOption)
            .Description("some option")
            .Build();

        rootCommandMock.Setup(x => x.GetOption(someOption))
            .Returns(
                option);

        rootCommandMock.Setup(x => x.Options).Returns(new ClyshMap<ClyshOption>() { { someOption, option } });
        cli.Execute(args);

        viewMock.Verify(
            x => x.PrintException(
                It.Is<ValidationException>(y =>
                    y.Message == $"The parameter data 'testarg' is out of bound for option: {someOption}.")),
            Times.Once);
    }

    [Test]
    public void ExecuteRootWithSomeOptionAndParameterByPositionAlreadyFilledError()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes, "mytest", "testarg:mytest" };

        var builder = new ClyshCommandBuilder();

        IClyshCommand rootCommand = builder.Id("root").Description("root command").Action(EmptyAction).Build();

        IClyshService cli = new ClyshService(rootCommand, viewMock.Object);

        var optionBuilder = new ClyshOptionBuilder();

        cli.RootCommand.AddOption(optionBuilder
            .Id(someOption)
            .Description(someOptionDescription)
            .Parameter(parameterBuilder.Id("testarg").Range(6, 10).Required(true).Build())
            .Build());

        cli.Execute(args);

        viewMock.Verify(
            x => x.PrintException(
                It.Is<ValidationException>(y =>
                    y.Message == $"The parameter 'testarg' is already filled for option: {someOption}.")), Times.Once);
    }

    [Test]
    public void SuccessfulExecuteRootWithGroupOptionDefault()
    {
        var args = new[] { " " };

        var someOption = "dev";

        ClyshMap<ClyshOption>? expectedOptions = null;
        IClyshView? expectedCliFront = null;

        void Action(IClyshCommand command, ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        var optBuilder = new ClyshOptionBuilder();

        var groupBuilder = new ClyshGroupBuilder();

        var group = groupBuilder
            .Id("env")
            .Build();

        var devOption = optBuilder
            .Id("dev")
            .Group(group)
            .Selected(true)
            .Build();

        var homOption = optBuilder
            .Id("hom")
            .Group(group)
            .Build();

        IClyshCommand rootCommand = builder
            .Id("root")
            .Description("root command")
            .Group(group)
            .Action(Action)
            .Option(devOption)
            .Option(homOption)
            .Build();

        IClyshService cli = new ClyshService(rootCommand, viewMock.Object);

        cli.Execute(args);

        Assert.NotNull(expectedOptions);
        Assert.NotNull(expectedCliFront);

        Assert.AreEqual(1, expectedOptions?.Count(x => x.Value.Selected));
        Assert.IsTrue(expectedOptions?[someOption].Selected);
        Assert.AreEqual(someOption, expectedOptions?[someOption].Id);

        Assert.AreEqual(cli.View, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithGroupOptionPassed()
    {
        var someOption = "hom";
        var someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes };

        ClyshMap<ClyshOption>? expectedOptions = null;
        IClyshView? expectedCliFront = null;

        void Action(IClyshCommand command, ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        var optBuilder = new ClyshOptionBuilder();

        var groupBuilder = new ClyshGroupBuilder();

        var group = groupBuilder
            .Id("env")
            .Build();

        var devOption = optBuilder
            .Id("dev")
            .Group(group)
            .Selected(true)
            .Build();

        var homOption = optBuilder
            .Id("hom")
            .Group(group)
            .Build();

        IClyshCommand rootCommand = builder
            .Id("root")
            .Description("root command")
            .Action(Action)
            .Group(group)
            .Option(devOption)
            .Option(homOption)
            .Build();

        IClyshService cli = new ClyshService(rootCommand, viewMock.Object);

        cli.Execute(args);

        Assert.NotNull(expectedOptions);
        Assert.NotNull(expectedCliFront);

        Assert.AreEqual(1, expectedOptions?.Count(x => x.Value.Selected));
        Assert.IsTrue(expectedOptions?[someOption].Selected);
        Assert.AreEqual(someOption, expectedOptions?[someOption].Id);

        Assert.AreEqual(cli.View, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithGroupOptionPassedAndNoDefault()
    {
        var someOption = "hom";
        var someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes };

        ClyshMap<ClyshOption>? expectedOptions = null;
        IClyshView? expectedCliFront = null;

        void Action(IClyshCommand command, ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        var optBuilder = new ClyshOptionBuilder();

        var groupBuilder = new ClyshGroupBuilder();

        var group = groupBuilder
            .Id("env")
            .Build();

        var devOption = optBuilder
            .Id("dev")
            .Group(group)
            .Build();

        var homOption = optBuilder
            .Id("hom")
            .Group(group)
            .Build();

        IClyshCommand rootCommand = builder
            .Id("root")
            .Description("root command")
            .Action(Action)
            .Group(group)
            .Option(devOption)
            .Option(homOption)
            .Build();

        IClyshService cli = new ClyshService(rootCommand, viewMock.Object);

        cli.Execute(args);

        Assert.NotNull(expectedOptions);
        Assert.NotNull(expectedCliFront);

        Assert.AreEqual(1, expectedOptions?.Count(x => x.Value.Selected));
        Assert.IsTrue(expectedOptions?[someOption].Selected);
        Assert.AreEqual(someOption, expectedOptions?[someOption].Id);

        Assert.AreEqual(cli.View, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithGroupOptionPassedAndOtherOption()
    {
        var someOption = "hom";
        var someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes, "--opt2" };

        ClyshMap<ClyshOption>? expectedOptions = null;
        IClyshView? expectedCliFront = null;

        void Action(IClyshCommand command, ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        var optBuilder = new ClyshOptionBuilder();

        var groupBuilder = new ClyshGroupBuilder();

        var group = groupBuilder
            .Id("env")
            .Build();

        IClyshCommand rootCommand = builder
            .Id("root")
            .Description("root command")
            .Action(Action)
            .Group(group)
            .Option(optBuilder
                .Id("dev")
                .Group(group)
                .Build())
            .Option(optBuilder
                .Id("hom")
                .Group(group)
                .Build())
            .Option(optBuilder
                .Id("opt2")
                .Build())
            .Build();

        IClyshService cli = new ClyshService(rootCommand, viewMock.Object);

        cli.Execute(args);

        Assert.NotNull(expectedOptions);
        Assert.NotNull(expectedCliFront);

        Assert.AreEqual(2, expectedOptions?.Count(x => x.Value.Selected));
        Assert.IsTrue(expectedOptions?[someOption].Selected);
        Assert.IsTrue(expectedOptions?["opt2"].Selected);
        Assert.AreEqual(someOption, expectedOptions?[someOption].Id);
        Assert.AreEqual("opt2", expectedOptions?["opt2"].Id);

        Assert.AreEqual(cli.View, expectedCliFront);
    }

    [Test]
    public void SuccessfulExecuteRootWithGroupOptionPassedAndOtherOptionWithParameter()
    {
        var someOption = "hom";
        var someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes, "--opt2", "testarg" };

        ClyshMap<ClyshOption>? expectedOptions = null;
        IClyshView? expectedCliFront = null;

        void Action(IClyshCommand command, ClyshMap<ClyshOption> options, IClyshView view)
        {
            expectedOptions = options;
            expectedCliFront = view;
        }

        var builder = new ClyshCommandBuilder();

        var optBuilder = new ClyshOptionBuilder();

        var groupBuilder = new ClyshGroupBuilder();

        var group = groupBuilder
            .Id("env")
            .Build();

        IClyshCommand rootCommand = builder
            .Id("root")
            .Description("root command")
            .Action(Action)
            .Group(group)
            .Option(optBuilder
                .Id("dev")
                .Group(group)
                .Build())
            .Option(optBuilder
                .Id("hom")
                .Group(group)
                .Build())
            .Option(optBuilder
                .Id("opt2")
                .Parameter(parameterBuilder.Id("testarg").Range(6, 10).Required(true).Build())
                .Build())
            .Build();

        IClyshService cli = new ClyshService(rootCommand, viewMock.Object);

        cli.Execute(args);

        Assert.NotNull(expectedOptions);
        Assert.NotNull(expectedCliFront);

        Assert.AreEqual(2, expectedOptions?.Count(x => x.Value.Selected));
        Assert.IsTrue(expectedOptions?[someOption].Selected);
        Assert.IsTrue(expectedOptions?["opt2"].Selected);
        Assert.AreEqual(someOption, expectedOptions?[someOption].Id);
        Assert.AreEqual("opt2", expectedOptions?["opt2"].Id);
        Assert.AreEqual("testarg", expectedOptions?["opt2"].Parameters["testarg"].Data);

        Assert.AreEqual(cli.View, expectedCliFront);
    }
}