using System;

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
                .Action((options, cliFront) =>
                {
                    if (options.Has(developmentOption))
                        cliFront.Print("Selected environment: development");

                    if (options.Has(homologOption))
                        cliFront.Print("Selected environment: homolog");

                    if (options.Has(productionOption))
                        cliFront.Print("Selected environment: production");
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
                .Action((options, cliFront) =>
                {
                    if (options.Has(appNameOption))
                    {
                        var appname = options[appNameOption];

                        cliFront.Print("appname: " + appname.Parameters[appNameOption].Data);
                    }
                    else
                    {
                        var guid = Guid.NewGuid();
                        cliFront.Print("appname: (random) " + guid.ToString());
                    }

                    if (options.Has(scopeOption))
                    {
                        cliFront.Print("scope: " + options[scopeOption].Parameters["scope"].Data);
                        cliFront.Print("tags: " + options[scopeOption].Parameters["tags"].Data);
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
                        new("scope", 1, 1000),
                        new("tags", 1, 1000, false)))
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
                .Action((_, _) =>
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
                .Action((options, cliFront) =>
                {
                    if (options.Has(promptOption))
                    {
                        cliFront.Print($"Your username is: {cliFront.AskFor("Username")}");
                        cliFront.Print($"Your password is: {cliFront.AskFor("Password")}");
                    }
                    else if (options.Has(credentialsOption))
                    {
                        var credential = options[credentialsOption];
                        cliFront.Print("Your credential path is: " + credential.Parameters["path"].Data);
                    }

                    if (cliFront.Confirm("Salvar login?", "Sim", "Nao"))
                        cliFront.Print("OK");
                    else
                        cliFront.Print("Aborted");
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