namespace CliSharp.Example
{
    public static class CliProgram
    {
        public static void Main(string[] args)
        {
            CliSharpDataSetup setup = new("clidata.yml");

            setup.MakeAction("mycli", (options, view) =>
            {
                view.Print(options.Has("test") ? "mycli with test option" : "mycli without test option");
            });
            
            ICliSharpService cli = new CliSharpService(setup);

            cli.Execute(args);
        }
    }
}