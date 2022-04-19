using System;

namespace CliSharp.Tests
{
    public static class CLiConfigForTest
    {
        public static ICliSharpCommand CreateRootCommand()
        {
            const string DEVELOPMENT_OPTION = "development";
            const string HOMOLOG_OPTION = "homolog";
            const string PRODUCTION_OPTION = "production";

            ICliSharpCommand login = CreateLoginCommand();
            ICliSharpCommand credential = CreateCredentialCommand();

            return CliSharpCommand.Create(
                "auth2",
                "Execute Auth 2 API CLI Application",
                (options, cliFront) =>
                {
                    if (options.Has(DEVELOPMENT_OPTION))
                        cliFront.PrintWithBreak("Selected environment: development");

                    if (options.Has(HOMOLOG_OPTION))
                        cliFront.PrintWithBreak("Selected environment: homolog");

                    if (options.Has(PRODUCTION_OPTION))
                        cliFront.PrintWithBreak("Selected environment: production");
                })
                .AddOption(DEVELOPMENT_OPTION, "Development environment option. Default value.", "d")
                .AddOption(HOMOLOG_OPTION, "Homolog environment option", "h")
                .AddOption(PRODUCTION_OPTION, "Production environment option", "p")
                .AddCommand(login)
                .AddCommand(credential);
        }

        private static ICliSharpCommand CreateCredentialCommand()
        {
            const string APP_NAME_OPTION = "app-name";
            const string SCOPE_OPTION = "scope";

            return CliSharpCommand.Create(
                "credential",
                "Manager a credential",
                (options, cliFront) =>
                {
                    if (options.Has(APP_NAME_OPTION))
                    {
                        CliSharpOption appname = options.GetByName(APP_NAME_OPTION);

                        cliFront.PrintWithBreak("appname: " + appname.Parameters.Get(APP_NAME_OPTION).Data);
                    }
                    else
                    {
                        Guid guid = Guid.NewGuid();
                        cliFront.PrintWithBreak("appname: (random) " + guid.ToString());
                    }

                    if (options.Has(SCOPE_OPTION))
                    {
                        cliFront.PrintWithBreak("scope: " + options.GetByName(SCOPE_OPTION).Parameters.Itens[0].Data);
                        cliFront.PrintWithBreak("tags: " + options.GetByName(SCOPE_OPTION).Parameters.Itens[1].Data);
                    }
                })
                .AddOption(APP_NAME_OPTION, "Name of the app", CliSharpParameters.Create(
                    new CliSharpParameter("app-name", 1, 100)))
                .AddOption(SCOPE_OPTION, "Scopes of the app by comma", CliSharpParameters.Create(
                        new("scope", 1, 1000),
                        new("tags", 1, 1000, false)))
                .AddCommand(CreateTestCredentialCommand());
        }

        private static ICliSharpCommand CreateTestCredentialCommand()
        {
            const string TIME_OPTION = "time-to-expire";

            return CliSharpCommand.Create(
                "test",
                "Test credential command",
                (options, cliFront) => 
                {

                }
            )
            .AddOption(TIME_OPTION, "time to expire credential in hours.", "t", CliSharpParameters.Create(new CliSharpParameter("hours", 1, 2)));
        }

        private static ICliSharpCommand CreateLoginCommand()
        {
            const string PROMPT_OPTION = "prompt";
            const string CREDENTIALS_OPTION = "credentials";

            return CliSharpCommand.Create(
                "login",
                "User login command for system",
                (options, cliFront) =>
                {
                    if (options.Has(PROMPT_OPTION))
                    {
                        cliFront.PrintWithBreak($"Your username is: {cliFront.AskFor("Username")}");
                        cliFront.PrintWithBreak($"Your password is: {cliFront.AskFor("Password")}");
                    }
                    else if (options.Has(CREDENTIALS_OPTION))
                    {
                        CliSharpOption credential = options.GetByName(CREDENTIALS_OPTION);
                        cliFront.PrintWithBreak("Your credential path is: " + credential.Parameters.Itens[0].Data);
                    }

                    if (cliFront.Confirm("Salvar login?", "Sim", "Nao"))
                        cliFront.PrintWithBreak("OK");
                    else
                        cliFront.PrintWithBreak("Aborted");
                })
                .AddOption(PROMPT_OPTION, "Prompt your credentials", "p")
                .AddOption(CREDENTIALS_OPTION, "Your username credentials path", "c", CliSharpParameters.Create(new CliSharpParameter("path", 1, 10)));
        }
    }
}