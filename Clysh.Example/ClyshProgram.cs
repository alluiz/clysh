namespace Clysh.Example
{
    public static class ClyshProgram
    {
        public static void Main(string[] args)
        {
            try
            {
                IClyshService cli = default!;

                try
                {
                    ClyshSetup setup = new("clidata.yml");

                    setup.MakeAction("mycli",
                        (options, view) =>
                        {
                            view.Print(options.Has("test") ? "mycli with test option" : "mycli without test option");

                            if (options.Has("test"))
                            {
                                var option = options["test"];

                                var data = option.Parameters["ab"].Data;

                                view.Print(data);
                            }
                        });

                    cli = new ClyshService(setup);
                }
                catch (ClyshException e)
                {
                    Console.Write(e);
                    Environment.ExitCode = 2;
                }

                cli.Execute(args);
            }
            catch (Exception e)
            {
                Console.Write(e);
                Environment.ExitCode = 1;
            }
        }
    }
}