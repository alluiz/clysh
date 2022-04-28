using System;

namespace Clysh.Example
{
    public static class ClyshProgram
    {
        public static void Main(string[] args)
        {
            try
            {
                ClyshDataSetup setup = new("clidata.yml");

                setup.MakeAction("mycli", (options, view) =>
                {
                    view.Print(options.Has("test") ? "mycli with test option" : "mycli without test option");
                });
            
                IClyshService cli = new ClyshService(setup);

                cli.Execute(args);
            }
            catch (ArgumentNullException e)
            {
                Console.Write(e);
                Environment.ExitCode = 3;
            }
            catch (ArgumentException e)
            {
                Console.Write(e);
                Environment.ExitCode = 2;
            }
            catch (Exception e)
            {
                Console.Write(e);
                Environment.ExitCode = 1;
            }
        }
    }
}