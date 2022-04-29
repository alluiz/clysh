using System;

namespace Clysh.Tests
{
    public static class ClyshDataForTest
    {
        public static IClyshCommand CreateRootCommand()
        {
            const string DEVELOPMENT_OPTION = "development";
            const string HOMOLOG_OPTION = "homolog";
            const string PRODUCTION_OPTION = "production";

            ClyshCommand login = CreateLoginCommand();
            ClyshCommand credential = CreateCredentialCommand();

            ClyshCommandBuilder builder = new ClyshCommandBuilder();
            ClyshOptionBuilder optionBuilder = new ClyshOptionBuilder();

           return builder
                .Id("auth2")
                .Description("Execute Auth 2 API CLI Application")
                .Action((options, cliFront) =>
                {
                    if (options.Has(DEVELOPMENT_OPTION))
                        cliFront.Print("Selected environment: development");

                    if (options.Has(HOMOLOG_OPTION))
                        cliFront.Print("Selected environment: homolog");

                    if (options.Has(PRODUCTION_OPTION))
                        cliFront.Print("Selected environment: production");
                })
                .Option(optionBuilder.Id(DEVELOPMENT_OPTION)
                    .Description("Development environment option. Default value.")
                    .Shortcut("d")
                    .Build())
                .Option(optionBuilder.Id(HOMOLOG_OPTION)
                    .Description("Homolog environment option.")
                    .Shortcut("s")
                    .Build())
                .Option(optionBuilder.Id(PRODUCTION_OPTION)
                    .Description("Production environment option.")
                    .Shortcut("p")
                    .Build())
                .Child(login)
                .Child(credential)
                .Build();
        }

        private static ClyshCommand CreateCredentialCommand()
        {
            const string APP_NAME_OPTION = "app-name";
            const string SCOPE_OPTION = "scope";
            
            ClyshCommandBuilder builder = new ClyshCommandBuilder();
            ClyshOptionBuilder optionBuilder = new ClyshOptionBuilder();
            
            return builder
                .Id("credential")
                .Description("Manager a credential")
                .Action((options, cliFront) =>
                {
                    if (options.Has(APP_NAME_OPTION))
                    {
                        ClyshOption appname = options.Get(APP_NAME_OPTION);

                        cliFront.Print("appname: " + appname.Parameters.Get(APP_NAME_OPTION).Data);
                    }
                    else
                    {
                        Guid guid = Guid.NewGuid();
                        cliFront.Print("appname: (random) " + guid.ToString());
                    }

                    if (options.Has(SCOPE_OPTION))
                    {
                        cliFront.Print("scope: " + options.Get(SCOPE_OPTION).Parameters.Itens[0].Data);
                        cliFront.Print("tags: " + options.Get(SCOPE_OPTION).Parameters.Itens[1].Data);
                    }
                })
                .Option(optionBuilder.Id(APP_NAME_OPTION)
                    .Description("Name of the app")
                    .Parameters(ClyshParameters.Create(
                        new ClyshParameter("app-name", 1, 100)))
                    .Build())
                .Option(optionBuilder.Id(SCOPE_OPTION)
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
            const string TIME_OPTION = "time-to-expire";
            
            ClyshCommandBuilder builder = new ClyshCommandBuilder();
            ClyshOptionBuilder optionBuilder = new ClyshOptionBuilder();
            
            return builder
                .Id("test")
                .Description("Test credential command")
                .Action((options, cliFront) =>
                {
                    
                })
                .Option(optionBuilder.Id(TIME_OPTION)
                    .Description("time to expire credential in hours.")
                    .Shortcut("t")
                    .Parameters(ClyshParameters.Create(new ClyshParameter("hours", 1, 2)))
                    .Build())
                .Build();
        }

        private static ClyshCommand CreateLoginCommand()
        {
            const string PROMPT_OPTION = "prompt";
            const string CREDENTIALS_OPTION = "credentials";
            
            ClyshCommandBuilder builder = new ClyshCommandBuilder();
            ClyshOptionBuilder optionBuilder = new ClyshOptionBuilder();
            
            return builder
                .Id("login")
                .Description("User login command for system")
                .Action((options, cliFront) =>
                {
                    if (options.Has(PROMPT_OPTION))
                    {
                        cliFront.Print($"Your username is: {cliFront.AskFor("Username")}");
                        cliFront.Print($"Your password is: {cliFront.AskFor("Password")}");
                    }
                    else if (options.Has(CREDENTIALS_OPTION))
                    {
                        ClyshOption credential = options.Get(CREDENTIALS_OPTION);
                        cliFront.Print("Your credential path is: " + credential.Parameters.Itens[0].Data);
                    }

                    if (cliFront.Confirm("Salvar login?", "Sim", "Nao"))
                        cliFront.Print("OK");
                    else
                        cliFront.Print("Aborted");
                })
                .Option(optionBuilder.Id(PROMPT_OPTION)
                    .Description("Prompt your credentials")
                    .Shortcut("p")
                    .Build())
                .Option(optionBuilder.Id(CREDENTIALS_OPTION)
                    .Description("Your username credentials path")
                    .Shortcut("c")
                    .Parameters(ClyshParameters.Create(new ClyshParameter("path", 1, 10)))
                    .Build())
                .Build();
        }
    }
}