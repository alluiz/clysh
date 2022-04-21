namespace CliSharp.Example
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
            
            ICliSharpService cli = new CliSharpService(setup.RootCommand, new CliSharpConsole(), setup.Data);

            cli.Execute(args);
        }
    }
}