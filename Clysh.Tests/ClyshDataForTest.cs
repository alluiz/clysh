using System;
using Clysh.Core;

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
                .Option(optionBuilder.Id(developmentOption)
                    .Description("Development environment option. Default value.")
                    .Shortcut("d")
                    .Build())
                .Option(optionBuilder.Id(homologOption)
                    .Description("Homolog environment option.")
                    .Shortcut("s")
                    .Build())
                .Option(optionBuilder.Id(productionOption)
                    .Description("Production environment option.")
                    .Shortcut("p")
                    .Build())
                .Child(login)
                .Child(credential)
                .Build();
        }

        private static ClyshCommand CreateCredentialCommand()
        {
            const string appNameOption = "app-name";
            const string scopeOption = "scope";
            
            var builder = new ClyshCommandBuilder();
            var optionBuilder = new ClyshOptionBuilder();
            
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
                    .Parameters(ClyshParameters.Create(
                        new ClyshParameter("app-name", 1, 100)))
                    .Build())
                .Option(optionBuilder.Id(scopeOption)
                    .Description("Scopes of the app by comma")
                    .Parameters(ClyshParameters.Create(
                        new ClyshParameter("scope", 1, 1000),
                        new ClyshParameter("tags", 1, 1000, false)))
                    .Build())
                .Child(CreateTestCredentialCommand())
                .Build();
        }

        private static ClyshCommand CreateTestCredentialCommand()
        {
            const string timeOption = "time-to-expire";
            
            var builder = new ClyshCommandBuilder();
            var optionBuilder = new ClyshOptionBuilder();
            
            return builder
                .Id("test")
                .Description("Test credential command")
                .Action((_, _, _) =>
                {
                    
                })
                .Option(optionBuilder.Id(timeOption)
                    .Description("time to expire credential in hours.")
                    .Shortcut("t")
                    .Parameters(ClyshParameters.Create(new ClyshParameter("hours", 1, 2)))
                    .Build())
                .Build();
        }

        private static ClyshCommand CreateLoginCommand()
        {
            const string promptOption = "prompt";
            const string credentialsOption = "credentials";
            
            var builder = new ClyshCommandBuilder();
            var optionBuilder = new ClyshOptionBuilder();
            
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
                    .Id(promptOption)
                    .Description("Prompt your credentials")
                    .Shortcut("p")
                    .Build())
                .Option(optionBuilder
                    .Id(credentialsOption)
                    .Description("Your username credentials path")
                    .Shortcut("c")
                    .Parameters(ClyshParameters.Create(new ClyshParameter("path", 1, 10)))
                    .Build())
                .Build();
        }
    }
}