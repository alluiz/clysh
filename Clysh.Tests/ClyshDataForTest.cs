using System;
using Clysh.Core;
using Clysh.Core.Builder;

namespace Clysh.Tests
{
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

            return builder
                .Id("auth2")
                .Description("Execute Auth 2 API CLI Application")
                .Action((_, options, view) =>
                {
                    if (options[developmentOption].Selected)
                        view.Print("Selected environment: development");

                    if (options[homologOption].Selected)
                        view.Print("Selected environment: homolog");

                    if (options[productionOption].Selected)
                        view.Print("Selected environment: production");
                })
                .Option(optionBuilder.Id(developmentOption, "d")
                    .Description("Development environment option. Default value.")
                    .Build())
                .Option(optionBuilder.Id(homologOption, "s")
                    .Description("Homolog environment option.")
                    .Build())
                .Option(optionBuilder.Id(productionOption, "p")
                    .Description("Production environment option.")
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
                .Id("credential")
                .Description("Manager a credential")
                .Action((_, options, view) =>
                {
                    if (options[appNameOption].Selected)
                    {
                        var appname = options[appNameOption];

                        view.Print("appname: " + appname.Parameters[appNameOption].Data);
                    }
                    else
                    {
                        var guid = Guid.NewGuid();
                        view.Print("appname: (random) " + guid.ToString());
                    }

                    if (options[scopeOption].Selected)
                    {
                        view.Print("scope: " + options[scopeOption].Parameters["scope"].Data);
                        view.Print("tags: " + options[scopeOption].Parameters["tags"].Data);
                    }
                })
                .Option(optionBuilder.Id(appNameOption)
                    .Description("Name of the app")
                    .Parameter(parameterBuilder.Id("app-name").Range(1, 100).Required(true).Build())
                    .Build())
                .Option(optionBuilder.Id(scopeOption)
                    .Description("Scopes of the app by comma")
                    .Parameter(parameterBuilder.Id("scope").Range(1, 1000).Required(true).Build())
                    .Parameter(parameterBuilder.Id("tags").Range(1, 1000).Required(false).Build())
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
                .Id("test")
                .Description("Test credential command")
                .Action((_, _, _) => { })
                .Option(optionBuilder.Id(timeOption, "t")
                    .Description("time to expire credential in hours.")
                    .Parameter(parameterBuilder.Id("hours").Range(1, 2).Required(true).Build())
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
                .Id("login")
                .Description("User login command for system")
                .Action((_, options, view) =>
                {
                    if (options[promptOption].Selected)
                    {
                        view.Print($"Your username is: {view.AskFor("Username")}");
                        view.Print($"Your password is: {view.AskFor("Password")}");
                    }
                    else if (options[credentialsOption].Selected)
                    {
                        var credential = options[credentialsOption];
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
                    .Parameter(parameterBuilder.Id("path").Range(1, 10).Required(true).Build())
                    .Build())
                .Build();
        }
    }
}