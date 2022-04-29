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

            IClyshCommand login = CreateLoginCommand();
            IClyshCommand credential = CreateCredentialCommand();

            return ClyshCommand.Create(
                "auth2",
                "Execute Auth 2 API CLI Application",
                (options, cliFront) =>
                {
                    if (options.Has(DEVELOPMENT_OPTION))
                        cliFront.Print("Selected environment: development");

                    if (options.Has(HOMOLOG_OPTION))
                        cliFront.Print("Selected environment: homolog");

                    if (options.Has(PRODUCTION_OPTION))
                        cliFront.Print("Selected environment: production");
                })
                .AddOption(DEVELOPMENT_OPTION, "Development environment option. Default value.", "d")
                .AddOption(HOMOLOG_OPTION, "Homolog environment option", "s")
                .AddOption(PRODUCTION_OPTION, "Production environment option", "p")
                .AddCommand(login)
                .AddCommand(credential);
        }

        private static IClyshCommand CreateCredentialCommand()
        {
            const string APP_NAME_OPTION = "app-name";
            const string SCOPE_OPTION = "scope";

            return ClyshCommand.Create(
                "credential",
                "Manager a credential",
                (options, cliFront) =>
                {
                    if (options.Has(APP_NAME_OPTION))
                    {
                        ClyshOption appname = options.GetByName(APP_NAME_OPTION);

                        cliFront.Print("appname: " + appname.Parameters.Get(APP_NAME_OPTION).Data);
                    }
                    else
                    {
                        Guid guid = Guid.NewGuid();
                        cliFront.Print("appname: (random) " + guid.ToString());
                    }

                    if (options.Has(SCOPE_OPTION))
                    {
                        cliFront.Print("scope: " + options.GetByName(SCOPE_OPTION).Parameters.Itens[0].Data);
                        cliFront.Print("tags: " + options.GetByName(SCOPE_OPTION).Parameters.Itens[1].Data);
                    }
                })
                .AddOption(APP_NAME_OPTION, "Name of the app", ClyshParameters.Create(
                    new ClyshParameter("app-name", 1, 100)))
                .AddOption(SCOPE_OPTION, "Scopes of the app by comma", ClyshParameters.Create(
                        new("scope", 1, 1000),
                        new("tags", 1, 1000, false)))
                .AddCommand(CreateTestCredentialCommand());
        }

        private static IClyshCommand CreateTestCredentialCommand()
        {
            const string TIME_OPTION = "time-to-expire";

            return ClyshCommand.Create(
                "test",
                "Test credential command",
                (options, cliFront) => 
                {

                }
            )
            .AddOption(TIME_OPTION, "time to expire credential in hours.", "t", ClyshParameters.Create(new ClyshParameter("hours", 1, 2)));
        }

        private static IClyshCommand CreateLoginCommand()
        {
            const string PROMPT_OPTION = "prompt";
            const string CREDENTIALS_OPTION = "credentials";

            return ClyshCommand.Create(
                "login",
                "User login command for system",
                (options, cliFront) =>
                {
                    if (options.Has(PROMPT_OPTION))
                    {
                        cliFront.Print($"Your username is: {cliFront.AskFor("Username")}");
                        cliFront.Print($"Your password is: {cliFront.AskFor("Password")}");
                    }
                    else if (options.Has(CREDENTIALS_OPTION))
                    {
                        ClyshOption credential = options.GetByName(CREDENTIALS_OPTION);
                        cliFront.Print("Your credential path is: " + credential.Parameters.Itens[0].Data);
                    }

                    if (cliFront.Confirm("Salvar login?", "Sim", "Nao"))
                        cliFront.Print("OK");
                    else
                        cliFront.Print("Aborted");
                })
                .AddOption(PROMPT_OPTION, "Prompt your credentials", "p")
                .AddOption(CREDENTIALS_OPTION, "Your username credentials path", "c", ClyshParameters.Create(new ClyshParameter("path", 1, 10)));
        }
    }
}