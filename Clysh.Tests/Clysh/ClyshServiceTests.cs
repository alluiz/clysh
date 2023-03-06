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
    private ClyshCommandBuilder _builder = new();
    private ClyshOptionBuilder _optionBuilder = new();
    private ClyshGroupBuilder _groupBuilder = new();
    private ClyshParameterBuilder _parameterBuilder = new();
    private readonly Mock<IClyshView> _viewMock = new();
    private bool _executed = false;


    [SetUp]
    public void Setup()
    {
        _builder = new ClyshCommandBuilder();
        _optionBuilder = new ClyshOptionBuilder();
        _groupBuilder = new ClyshGroupBuilder();
        _parameterBuilder = new ClyshParameterBuilder();
        _executed = false;
        _viewMock.Reset();
    }

    [Test]
    public void SuccessfulCreateCliAndRootCommand()
    {
        
        var command = _builder
            .Id("test")
            .Description("test command description")
            .Build();

        IClyshService cli = new ClyshService(command, _viewMock.Object);
        Assert.AreEqual(command, cli.RootCommand);
        Assert.AreEqual(_viewMock.Object, cli.View);
    }

    [Test]
    public void SuccessfulCreateCommand()
    {
        const string name = "new";
        const string description = "new command for test";

        IClyshCommand command = _builder
            .Id(name)
            .Description(description)
            .Action(EmptyAction)
            .Build();

        Assert.AreEqual(name, command.Id);
        Assert.AreEqual(description, command.Description);
        Assert.AreEqual((Action<IClyshCommand, IClyshView>)EmptyAction, command.Action);
    }

    private void EmptyAction(IClyshCommand clyshCommand, IClyshView clyshView)
    {
        _executed = true;
    }


    [Test]
    public void SuccessfulExecuteRootWithNoArgs()
    {
        var args = Array.Empty<string>();

        
        var command = _builder
            .Id("test")
            .Description("test command description")
            .Action(EmptyAction)
            .Build();

        IClyshService cli = new ClyshService(command, _viewMock.Object);
        
        cli.Execute(args);

        Assert.IsTrue(_executed);
    }

    [Test]
    public void SuccessfulExecuteRootWithOneEmptyArg()
    {
        var args = new[] { "" };

        
        var command = _builder
            .Id("test")
            .Description("test command description")
            .Action(EmptyAction)
            .Build();

        IClyshService cli = new ClyshService(command, _viewMock.Object);

        cli.Execute(args);

        Assert.IsTrue(_executed);
    }

    [Test]
    public void SuccessfulExecuteRootWithOneSpaceArg()
    {
        var args = new[] { "  " };

        
        var command = _builder
            .Id("test")
            .Description("test command description")
            .Action(EmptyAction)
            .Build();

        IClyshService cli = new ClyshService(command, _viewMock.Object);

        cli.Execute(args);

        Assert.IsTrue(_executed);
    }

    [Test]
    public void SuccessfulExecuteRootWithMultiSpacesArg()
    {
        var args = new[] { "  ", "   ", "         ", "  " };

        
        var command = _builder
            .Id("test")
            .Description("test command description")
            .Action(EmptyAction)
            .Build();

        IClyshService cli = new ClyshService(command, _viewMock.Object);

        cli.Execute(args);

        Assert.IsTrue(_executed);
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

        void Action(IClyshCommand command, IClyshView view)
        {
            expectedOptions = command.Options;
            expectedCliFront = view;
        }


        ClyshCommand rootCommand = _builder
            .Id("root")
            .Description("root command")
            .Action(Action)
            .Option(_optionBuilder
                .Id(someOption, someAbbrevOption)
                .Description(someOptionDescription)
                .Build())
            .Build();

        IClyshService cli = new ClyshService(rootCommand, _viewMock.Object);


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

        void Action(IClyshCommand command, IClyshView view)
        {
            expectedOptions = command.Options;
            expectedCliFront = view;
        }


        ClyshCommand rootCommand = _builder
            .Id("root")
            .Description("root command")
            .Action(Action)
            .Option(_optionBuilder
                .Id(someOption)
                .Description(someOptionDescription)
                .Parameter(_parameterBuilder
                    .Id("testarg")
                    .Range(6, 10)
                    .MarkAsRequired()
                    .Build())
                .Build())
            .Build();

        IClyshService cli = new ClyshService(rootCommand, _viewMock.Object);

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

        void Action(IClyshCommand command, IClyshView view)
        {
            expectedOptions = command.Options;
            expectedCliFront = view;
        }


        ClyshCommand rootCommand = _builder
            .Id("root")
            .Description("root command")
            .Action(Action)
            .Option(_optionBuilder
                .Id(someOption).Description(someOptionDescription)
                .Parameter(_parameterBuilder.Id("testarg").Range(6, 10).MarkAsRequired().Build())
                .Build())
            .Build();

        IClyshService cli = new ClyshService(rootCommand, _viewMock.Object);


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

        void Action(IClyshCommand command, IClyshView view)
        {
            expectedOptions = command.Options;
            expectedCliFront = view;
        }


        ClyshCommand rootCommand = _builder
            .Id("root")
            .Description("root command")
            .Action(Action)
            .Option(_optionBuilder
                .Id(someOption)
                .Description(someOptionDescription)
                .Parameter(_parameterBuilder.Id("testarg").Range(6, 10).Order(0).MarkAsRequired().Build())
                .Parameter(_parameterBuilder.Id("testarg2").Range(6, 10).Order(1).MarkAsRequired().Build())
                .Parameter(_parameterBuilder.Id("testarg3").Range(6, 10).Order(2).MarkAsRequired().Build())
                .Build())
            .Build();

        IClyshService cli = new ClyshService(rootCommand, _viewMock.Object);

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

        void Action(IClyshCommand command, IClyshView view)
        {
            expectedOptions = command.Options;
            expectedCliFront = view;
        }


        ClyshCommand rootCommand = _builder
            .Id("root")
            .Description("root command")
            .Action(Action)
            .Option(_optionBuilder
                .Id(someOption)
                .Description(someOptionDescription)
                .Parameter(_parameterBuilder
                    .Id("testarg")
                    .Range(6, 10)
                    
                    .Build())
                .Build())
            .Build();

        IClyshService cli = new ClyshService(rootCommand, _viewMock.Object);

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

        void Action(IClyshCommand command, IClyshView view)
        {
            expectedOptions = command.Options;
            expectedCliFront = view;
        }


        ClyshCommand rootCommand = _builder
            .Id("root")
            .Description("root command")
            .Action(Action)
            .Option(_optionBuilder
                .Id(someOption).Description(someOptionDescription)
                .Parameter(_parameterBuilder.Id("testarg2").Range(6, 10).Order(0).Build())
                .Parameter(_parameterBuilder.Id("testarg3").Range(6, 10).Order(1).Build())
                .Parameter(_parameterBuilder.Id("testarg").Range(6, 10).Order(2).Build())
                .Build())
            .Build();

        IClyshService cli = new ClyshService(rootCommand, _viewMock.Object);


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

        void Action(IClyshCommand command, IClyshView view)
        {
            expectedOptions = command.Options;
            expectedCliFront = view;
        }


        ClyshCommand rootCommand = _builder
            .Id("root")
            .Description("root command")
            .Action(Action)
            .Option(_optionBuilder
                .Id(someOption)
                .Description(someOptionDescription)
                .Parameter(_parameterBuilder.Id("testarg2").Range(6, 10).Order(0).MarkAsRequired().Build())
                .Parameter(_parameterBuilder.Id("testarg").Range(6, 10).Order(1).Build())
                .Build())
            .Build();

        IClyshService cli = new ClyshService(rootCommand, _viewMock.Object);

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

        void Action(IClyshCommand command, IClyshView view)
        {
            expectedOptions = command.Options;
            expectedCliFront = view;
        }


        ClyshCommand rootCommand = _builder
            .Id("root")
            .Description("root command")
            .Action(Action)
            .Option(_optionBuilder
                .Id(someOption)
                .Description(someOptionDescription)
                .Parameter(_parameterBuilder.Id("testarg5").Range(6, 10).Order(0).MarkAsRequired().Build())
                .Parameter(_parameterBuilder.Id("testarg2").Range(6, 10).Order(1).MarkAsRequired().Build())
                .Parameter(_parameterBuilder.Id("testarg").Range(6, 10).Order(2).Build())
                .Parameter(_parameterBuilder.Id("testarg3").Range(6, 10).Order(3).Build())
                .Parameter(_parameterBuilder.Id("testarg4").Range(6, 10).Order(4).Build())
                .Build())
            .Build();

        IClyshService cli = new ClyshService(rootCommand, _viewMock.Object);

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

        void Action(IClyshCommand command, IClyshView view)
        {
            expectedOptions = command.Options;
            expectedCliFront = view;
        }


        ClyshCommand rootCommand = _builder
            .Id("root")
            .Description("root command")
            .Action(Action)
            .Option(_optionBuilder
                .Id(someOption)
                .Description(someOptionDescription)
                .Parameter(_parameterBuilder.Id("testarg2").Range(6, 10).Order(1).MarkAsRequired().Build())
                .Parameter(_parameterBuilder.Id("testarg").Range(6, 10).Order(2).Build())
                .Build())
            .Option(_optionBuilder
                .Id(someOption2)
                .Description(someOptionDescription2)
                .Parameter(_parameterBuilder.Id("testarg5").Range(6, 10).Order(0).MarkAsRequired().Build())
                .Parameter(_parameterBuilder.Id("testarg3").Range(6, 10).Order(1).Build())
                .Parameter(_parameterBuilder.Id("testarg4").Range(6, 10).Order(2).Build())
                .Build())
            .Build();

        IClyshService cli = new ClyshService(rootCommand, _viewMock.Object);

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


        var customCommand = _builder
            .Id("root.test")
            .Description("test command description")
            .Action((command, view) =>
            {
                expectedOptionsCustom = command.Options;
                expectedCliFrontCustom = view;
            })
            .Option(_optionBuilder
                .Id(someOption2)
                .Description(someOptionDescription2)
                .Parameter(_parameterBuilder.Id("testarg5").Range(6, 10).MarkAsRequired().Order(0).Build())
                .Parameter(_parameterBuilder.Id("testarg3").Range(6, 10).Order(1).Build())
                .Parameter(_parameterBuilder.Id("testarg4").Range(6, 10).Order(2).Build())
                .Build())
            .Build();

        var rootCommand = _builder
            .Id("root")
            .Description("root command")
            .Action((command, view) =>
            {
                expectedOptions = command.Options;
                expectedCliFront = view;
            })
            .Option(_optionBuilder
                .Id(someOption)
                .Description(someOptionDescription)
                .Parameter(_parameterBuilder.Id("testarg2").Range(6, 10).Order(0).MarkAsRequired().Build())
                .Parameter(_parameterBuilder.Id("testarg").Range(6, 10).Order(1).Build())
                .Build())
            .SubCommand(customCommand)
            .Build();

        IClyshService cli = new ClyshService(rootCommand, _viewMock.Object);

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
    public void SuccessfulExecuteRootWithRebelCustomCommandAndMultiOptionAndMultiOptionalAndRequiredParameter()
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


        var customCommand = _builder
            .Id("root.test")
            .Description("test command description")
            .IgnoreParents()
            .Action((command, view) =>
            {
                expectedOptionsCustom = command.Options;
                expectedCliFrontCustom = view;
            })
            .Option(_optionBuilder
                .Id(someOption2)
                .Description(someOptionDescription2)
                .Parameter(_parameterBuilder.Id("testarg5").Range(6, 10).MarkAsRequired().Order(0).Build())
                .Parameter(_parameterBuilder.Id("testarg3").Range(6, 10).Order(1).Build())
                .Parameter(_parameterBuilder.Id("testarg4").Range(6, 10).Order(2).Build())
                .Build())
            .Build();

        var rootCommand = _builder
            .Id("root")
            .Description("root command")
            .Action((command, view) =>
            {
                //This code MUST BE not executed
                //Then, the expectedOptions is null
                expectedOptions = command.Options;
                expectedCliFront = view;
            })
            .Option(_optionBuilder
                .Id(someOption)
                .Description(someOptionDescription)
                .Parameter(_parameterBuilder.Id("testarg2").Range(6, 10).Order(0).MarkAsRequired().Build())
                .Parameter(_parameterBuilder.Id("testarg").Range(6, 10).Order(1).Build())
                .Build())
            .SubCommand(customCommand)
            .Build();

        IClyshService cli = new ClyshService(rootCommand, _viewMock.Object);

        cli.Execute(args);

        Assert.Null(expectedCliFront);
        Assert.Null(expectedOptions);

        Assert.AreEqual(1, expectedOptionsCustom?.Count(x => x.Value.Selected));
        Assert.IsTrue(expectedOptionsCustom?.Has(someOption2));

        Assert.AreEqual(someOption2, expectedOptionsCustom?[someOption2].Id);
        Assert.AreEqual(someOptionDescription2, expectedOptionsCustom?[someOption2].Description);
        Assert.AreEqual("mytest3", expectedOptionsCustom?[someOption2].Parameters["testarg3"].Data);
        Assert.AreEqual("mytest4", expectedOptionsCustom?[someOption2].Parameters["testarg4"].Data);
        Assert.AreEqual("mytest5", expectedOptionsCustom?[someOption2].Parameters["testarg5"].Data);

        Assert.AreEqual(cli.View, expectedCliFrontCustom);

        Assert.AreEqual(-1, cli.RootCommand.Order);
        Assert.AreEqual(0, customCommand.Order);
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

        void Action(IClyshCommand command, IClyshView view)
        {
            expectedOptions = command.Options;
            expectedCliFront = view;
        }


        ClyshCommand rootCommand = _builder
            .Id("root")
            .Description("root command")
            .Action(Action)
            .Option(_optionBuilder
                .Id(someOption, someAbbrevOption)
                .Description(someOptionDescription)
                .Build())
            .Build();

        IClyshService cli = new ClyshService(rootCommand, _viewMock.Object);

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
        
        
        var command = _builder.Id("test").Description("test command description").Build();

        IClyshService cli = new ClyshService(command, _viewMock.Object);

        cli.Execute(args);

        _viewMock.Verify(
            x => x.PrintException(
                It.Is<ValidationException>(
                    y => ClyshMessages.Match(y.Message, ClyshMessages.ErrorOnValidateUserInputOption,
                        invalidOptionWithDashes))), Times.Once);
    }

    [Test]
    public void ExecuteRootWithSomeInvalidAbbrevOptionError()
    {
        const string invalidOption = "i";

        const string invalidOptionWithDashes = $"-{invalidOption}";

        var args = new[] { invalidOptionWithDashes };
        
        
        var command = _builder.Id("test").Description("test command description").Build();

        IClyshService cli = new ClyshService(command, _viewMock.Object);

        cli.Execute(args);

        _viewMock.Verify(
            x => x.PrintException(
                It.Is<ValidationException>(
                    y => ClyshMessages.Match(y.Message, ClyshMessages.ErrorOnValidateUserInputOption,
                        invalidOptionWithDashes))), Times.Once);
    }

    [Test]
    public void ExecuteRootWithValidOptionButNoArgsError()
    {
        const string someOption = "some-option";

        const string someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes };
        
        const string arg = "testarg";

        var option = _optionBuilder
            .Id(someOption)
            .Description("some option")
            .Parameter(_parameterBuilder.Id(arg).Range(6, 10).MarkAsRequired().Build())
            .Build();
        
        
        var command = _builder
            .Id("test")
            .Description("test command description")
            .Option(option)
            .Build();

        IClyshService cli = new ClyshService(command, _viewMock.Object);
        
        cli.Execute(args);

        _viewMock.Verify(
            x => x.PrintException(
                It.Is<ValidationException>(y => ClyshMessages.Match(y.Message,
                    ClyshMessages.ErrorOnValidateUserInputRequiredParameters, arg, someOption, "<no_shortcut>"))),
            Times.Once);
    }

    [Test]
    public void ExecuteRootWithHelpOption()
    {
        const string helpOption = "help";

        const string helpOptionWithDashes = $"--{helpOption}";

        var args = new[] { helpOptionWithDashes };

        
        var command = _builder.Id("test").Description("test command description").Build();

        IClyshService cli = new ClyshService(command, _viewMock.Object);
        
        cli.Execute(args);

        _viewMock.Verify(x => x.PrintHelp(command), Times.Once);
    }

    [Test]
    public void ExecuteRootWithArgsAndNoOptionError()
    {
        var args = new[] { "testarg:mytest" };

        
        var command = _builder.Id("test").Description("test command description").Build();
        
        IClyshService cli = new ClyshService(command, _viewMock.Object);

        cli.Execute(args);

        _viewMock.Verify(
            x => x.PrintException(
                It.Is<ValidationException>(y =>
                    ClyshMessages.Match(y.Message, ClyshMessages.ErrorOnValidateUserInputArgument, args[0]))),
            Times.Once);
    }

    [Test]
    public void ExecuteRootWithSomeOptionAndInvalidArgsError()
    {
        const string someOption = "some-option";

        const string someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes, "testarg:mytest" };
        
        var option = _optionBuilder
            .Id(someOption)
            .Description("some option")
            .Build();
        
        
        var command = _builder
            .Id("test")
            .Description("test command description")
            .Option(option)
            .Build();

        IClyshService cli = new ClyshService(command, _viewMock.Object);
        
        cli.Execute(args);

        _viewMock.Verify(
            x => x.PrintException(
                It.Is<ValidationException>(y => ClyshMessages.Match(y.Message,
                    ClyshMessages.ErrorOnValidateUserInputParameterInvalid, "testarg", someOption))), Times.Once);
    }

    [Test]
    public void ExecuteRootWithSomeOptionAndInvalidArgsByPositionError()
    {
        const string someOption = "some-option";

        const string someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes, "testarg" };

        var option = _optionBuilder
            .Id(someOption)
            .Description("some option")
            .Build();
        
        
        var command = _builder
            .Id("test")
            .Description("test command description")
            .Option(option)
            .Build();

        IClyshService cli = new ClyshService(command, _viewMock.Object);
        
        cli.Execute(args);

        _viewMock.Verify(
            x => x.PrintException(
                It.Is<ValidationException>(y => ClyshMessages.Match(y.Message,
                    ClyshMessages.ErrorOnValidateUserInputArgumentOutOfBound, "testarg", someOption))),
            Times.Once);
    }

    [Test]
    public void ExecuteRootWithSomeOptionAndParameterByPositionAlreadyFilledError()
    {
        const string someOption = "some-option";
        const string someOptionDescription = "awesome option";

        const string someOptionWithDashes = $"--{someOption}";

        var args = new[] { someOptionWithDashes, "mytest", "testarg:mytest" };


        ClyshCommand rootCommand = _builder
            .Id("root")
            .Description("root command")
            .Action(EmptyAction)
            .Option(_optionBuilder
                .Id(someOption)
                .Description(someOptionDescription)
                .Parameter(_parameterBuilder.Id("testarg").Range(6, 10).MarkAsRequired().Build())
                .Build())
            .Build();

        IClyshService cli = new ClyshService(rootCommand, _viewMock.Object);

        cli.Execute(args);

        _viewMock.Verify(
            x => x.PrintException(
                It.Is<ValidationException>(y => ClyshMessages.Match(y.Message,
                    ClyshMessages.ErrorOnValidateUserInputParameterConflict, "testarg", someOption))), Times.Once);
    }

    [Test]
    public void SuccessfulExecuteRootWithGroupOptionDefault()
    {
        var args = new[] { " " };

        var someOption = "dev";

        ClyshMap<ClyshOption>? expectedOptions = null;
        IClyshView? expectedCliFront = null;

        void Action(IClyshCommand command, IClyshView view)
        {
            expectedOptions = command.Options;
            expectedCliFront = view;
        }

        var group = _groupBuilder
            .Id("env")
            .Build();

        var devOption = _optionBuilder
            .Id("dev")
            .Description("The dev option")
            .Group(group)
            .Selected(true)
            .Build();

        var homOption = _optionBuilder
            .Id("hom")
            .Description("The hom option")
            .Group(group)
            .Build();

        ClyshCommand rootCommand = _builder
            .Id("root")
            .Description("root command")
            .Action(Action)
            .Option(devOption)
            .Option(homOption)
            .Build();

        IClyshService cli = new ClyshService(rootCommand, _viewMock.Object);

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

        var args = new[] { "--dev", someOptionWithDashes };

        ClyshMap<ClyshOption>? expectedOptions = null;
        IClyshView? expectedCliFront = null;

        void Action(IClyshCommand command, IClyshView view)
        {
            expectedOptions = command.Options;
            expectedCliFront = view;
        }


        var group = _groupBuilder
            .Id("env")
            .Build();

        var devOption = _optionBuilder
            .Id("dev")
            .Description("The dev option")
            .Group(group)
            .Build();

        var homOption = _optionBuilder
            .Id("hom")
            .Description("The hom option")
            .Group(group)
            .Build();

        ClyshCommand rootCommand = _builder
            .Id("root")
            .Description("root command")
            .Action(Action)
            .Option(devOption)
            .Option(homOption)
            .Build();
        
        IClyshService cli = new ClyshService(rootCommand, _viewMock.Object);

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
        ClyshOption? selectedOptionFromGroup = null;

        void Action(IClyshCommand command, IClyshView view)
        {
            expectedOptions = command.Options;
            expectedCliFront = view;
            selectedOptionFromGroup = command.GetOptionFromGroup("env");
        }

        var group = _groupBuilder
            .Id("env")
            .Build();

        var devOption = _optionBuilder
            .Id("dev")
            .Group(group)
            .Description("The dev option")
            .Build();

        var homOption = _optionBuilder
            .Id("hom")
            .Group(group)
            .Description("The hom option")
            .Build();

        ClyshCommand rootCommand = _builder
            .Id("root")
            .Description("root command")
            .Action(Action)
            .Option(devOption)
            .Option(homOption)
            .Build();

        IClyshService cli = new ClyshService(rootCommand, _viewMock.Object);

        cli.Execute(args);

        Assert.NotNull(expectedOptions);
        Assert.NotNull(expectedCliFront);

        Assert.AreEqual(1, expectedOptions?.Count(x => x.Value.Selected));
        Assert.IsTrue(expectedOptions?[someOption].Selected);
        Assert.AreEqual(someOption, expectedOptions?[someOption].Id);
        Assert.AreEqual(homOption, selectedOptionFromGroup);

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

        void Action(IClyshCommand command, IClyshView view)
        {
            expectedOptions = command.Options;
            expectedCliFront = view;
        }


        var group = _groupBuilder
            .Id("env")
            .Build();

        ClyshCommand rootCommand = _builder
            .Id("root")
            .Description("root command")
            .Action(Action)
            .Option(_optionBuilder
                .Id("dev")
                .Description("The dev option")
                .Group(group)
                .Build())
            .Option(_optionBuilder
                .Id("hom")
                .Description("The hom option")
                .Group(group)
                .Build())
            .Option(_optionBuilder
                .Id("opt2")
                .Description("The opt2 option")
                .Build())
            .Build();

        IClyshService cli = new ClyshService(rootCommand, _viewMock.Object);

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

        void Action(IClyshCommand command, IClyshView view)
        {
            expectedOptions = command.Options;
            expectedCliFront = view;
        }

        var group = _groupBuilder
            .Id("env")
            .Build();

        ClyshCommand rootCommand = _builder
            .Id("root")
            .Description("root command")
            .Action(Action)
            .Option(_optionBuilder
                .Id("dev")
                .Description("The dev option")
                .Group(group)
                .Build())
            .Option(_optionBuilder
                .Id("hom")
                .Description("The hom option")
                .Group(group)
                .Build())
            .Option(_optionBuilder
                .Id("opt2")
                .Description("The opt2 option")
                .Parameter(_parameterBuilder.Id("testarg").Range(6, 10).MarkAsRequired().Build())
                .Build())
            .Build();

        IClyshService cli = new ClyshService(rootCommand, _viewMock.Object);

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