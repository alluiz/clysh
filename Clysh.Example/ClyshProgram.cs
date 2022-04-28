namespace Clysh.Example
{
    public static class ClyshProgram
    {
        public static void Main(string[] args)
        {
            ClyshDataSetup setup = new("clidata.yml");

            setup.MakeAction("mycli", (options, view) =>
            {
                view.Print(options.Has("test") ? "mycli with test option" : "mycli without test option");
            });
            
            IClyshService cli = new ClyshService(setup);

            cli.Execute(args);
        }
    }
}