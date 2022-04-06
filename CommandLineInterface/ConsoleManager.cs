namespace CommandLineInterface
{
    public interface IConsoleManager
    {
        void WriteLine(string text);
        void Separator();
        void EmptyLine();
        string? ReadLine();
        void Write(string text);
    }

    public class ConsoleManager : IConsoleManager
    {
        public void EmptyLine()
        {
            System.Console.WriteLine("");
        }

        public string? ReadLine()
        {
            return System.Console.ReadLine();
        }

        public void Separator()
        {
            System.Console.WriteLine("-----------#-----------");
        }

        public void Write(string text)
        {
            System.Console.Write(text);
        }

        public void WriteLine(string text)
        {
            System.Console.WriteLine(text);
        }
    }
}