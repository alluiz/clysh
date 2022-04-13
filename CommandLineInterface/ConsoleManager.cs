namespace CommandLineInterface
{
    public interface IConsoleManager
    {
        int CursorTop { get; set; }

        void WriteLine(string text = "");
        void Separator();
        void EmptyLine();
        string ReadLine();
        void Write(string text);
        string ReadSensitive();
    }

    public class ConsoleManager : IConsoleManager
    {
        public int CursorTop { get; set; }

        public void EmptyLine()
        {
            System.Console.WriteLine("");
        }

        public string ReadLine()
        {
            return System.Console.ReadLine() ?? "";
        }

        public void Separator()
        {
            System.Console.WriteLine("-----------#-----------");
        }

        public void Write(string text)
        {
            System.Console.Write(text);
        }

        public void WriteLine(string text = "")
        {
            System.Console.WriteLine(text);
        }

        public string ReadSensitive()
        {
            string data = "";

            ConsoleKeyInfo info = Console.ReadKey(true);

            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key != ConsoleKey.Backspace)
                {
                    Console.Write("*");
                    data += info.KeyChar;
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(data))
                    {
                        // remove one character from the list of password characters
                        data = data.Substring(0, data.Length - 1);
                        // get the location of the cursor
                        int pos = Console.CursorLeft;
                        // move the cursor to the left by one character
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                        // replace it with space
                        Console.Write(" ");
                        // move the cursor to the left by one character again
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    }
                }
                info = Console.ReadKey(true);
            }

            // add a new line because user pressed enter at the end of their password
            Console.WriteLine();

            return data;
        }
    }
}