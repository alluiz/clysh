using System;
using Clysh.Core;
using Clysh.Core.Builder;

namespace Clysh.Tests;

public static class ClyshDataForTest
{
    public static IClyshCommand CreateRootCommand()
    {
        const string developmentOption = "development";
        const string homologOption = "homolog";
        const string productionOption = "production";

        var login = CreateLoginCommand();
        var credential = CreateCredentialCommand();

        var builder = new ClyshCommandBuilder();
        var optionBuilder = new ClyshOptionBuilder();
        var groupBuilder = new ClyshGroupBuilder();

        var group = groupBuilder.Id("env").Build();
            
        return builder
            .Id("auth2")
            .Description("Execute Auth 2 API CLI Application")
            .Action((command, view) =>
            {
                var envOption = command.GetOptionFromGroup("env");

                if (envOption != null)
                {
                    if (envOption.Is("development"))
                        view.Print("Selected environment: development");
                    else if (envOption.Is("homolog"))
                        view.Print("Selected environment: homolog");
                    else
                        view.Print("Selected environment: production");
                }
            })
            .Option(optionBuilder.Id(developmentOption, "d")
                .Description("Development option.")
                .Group(group)
                .Build())
            .Option(optionBuilder.Id(homologOption, "s")
                .Description("Homolog environment option.")
                .Group(group)
                .Build())
            .Option(optionBuilder.Id(productionOption, "p")
                .Description("Production environment option.")
                .Group(group)
                .Build())
            .SubCommand(login)
            .SubCommand(credential)
            .Build();
    }

    private static ClyshCommand CreateCredentialCommand()
    {
        const string appNameOption = "app-name";
        const string scopeOption = "scope";

        var builder = new ClyshCommandBuilder();
        var optionBuilder = new ClyshOptionBuilder();
        var parameterBuilder = new ClyshParameterBuilder();

        return builder
            .Id("auth2.credential")
            .Description("Manager a credential")
            .Action((command, view) =>
            {
                if (command.Options[appNameOption].Selected)
                {
                    var appname = command.Options[appNameOption];

                    view.Print("appname: " + appname.Parameters[appNameOption].Data);
                }
                else
                {
                    var guid = Guid.NewGuid();
                    view.Print("appname: (random) " + guid.ToString());
                }

                if (command.Options[scopeOption].Selected)
                {
                    view.Print("scope: " + command.Options[scopeOption].Parameters["scope"].Data);
                    view.Print("tags: " + command.Options[scopeOption].Parameters["tags"].Data);
                }
            })
            .Option(optionBuilder.Id(appNameOption)
                .Description("Name of the app")
                .Parameter(parameterBuilder.Id("app-name").Range(1, 100).Required(true).Order(1).Build())
                .Build())
            .Option(optionBuilder.Id(scopeOption)
                .Description("Scopes of the app by comma")
                .Parameter(parameterBuilder.Id("scope").Range(1, 1000).Required(true).Order(1).Build())
                .Parameter(parameterBuilder.Id("tags").Range(1, 1000).Required(false).Order(2).Build())
                .Build())
            .SubCommand(CreateTestCredentialCommand())
            .Build();
    }

    private static ClyshCommand CreateTestCredentialCommand()
    {
        const string timeOption = "time-to-expire";

        var builder = new ClyshCommandBuilder();
        var optionBuilder = new ClyshOptionBuilder();
        var parameterBuilder = new ClyshParameterBuilder();

        return builder
            .Id("auth2.credential.test")
            .Description("Test credential command")
            .Action((_, _) => { })
            .Option(optionBuilder.Id(timeOption, "t")
                .Description("time to expire in hours.")
                .Parameter(parameterBuilder.Id("hours").Range(1, 2).Required(true).Order(1).Build())
                .Build())
            .Build();
    }

    private static ClyshCommand CreateLoginCommand()
    {
        const string promptOption = "prompt";
        const string credentialsOption = "credentials";

        var builder = new ClyshCommandBuilder();
        var optionBuilder = new ClyshOptionBuilder();
        var parameterBuilder = new ClyshParameterBuilder();

        return builder
            .Id("auth2.login")
            .Description("User login command for system")
            .Action((command, view) =>
            {
                if (command.Options[promptOption].Selected)
                {
                    view.Print($"Your username is: {view.AskFor("Username")}");
                    view.Print($"Your password is: {view.AskFor("Password")}");
                }
                else if (command.Options[credentialsOption].Selected)
                {
                    var credential = command.Options[credentialsOption];
                    view.Print("Your credential path is: " + credential.Parameters["path"].Data);
                }

                if (view.Confirm("Salvar login?", "Sim", "Nao"))
                    view.Print("OK");
                else
                    view.Print("Aborted");
            })
            .Option(optionBuilder
                .Id(promptOption, "p")
                .Description("Prompt your credentials")
                .Build())
            .Option(optionBuilder
                .Id(credentialsOption, "c")
                .Description("Your username credentials path")
                .Parameter(parameterBuilder.Id("path").Range(1, 10).Required(true).Order(1).Build())
                .Build())
            .Build();
    }
}