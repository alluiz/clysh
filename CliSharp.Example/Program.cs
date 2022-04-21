namespace CliSharp.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CliSharpDataSetup setup = new("clidata.yml");

            setup.SetCommandAction("mycli", (options, cliFront) =>
            {
                if (options.Has("test"))
                    cliFront.PrintWithBreak($"mycli with test option");
                else
                    cliFront.PrintWithBreak($"mycli without test option");
            });

            ICliSharpView view = new CliSharpView(new CliSharpConsole(), setup.Data, true);

            ICliSharpService cli = new CliSharpService(setup.RootCommand, view);

            cli.Execute(args);
        }
    }
}