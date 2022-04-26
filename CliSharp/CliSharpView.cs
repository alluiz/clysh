using CliSharp.Data;

namespace CliSharp
{
    public class CliSharpView : ICliSharpView
    {
        public CliSharpData Data { get; set; }
        public int PrintedLines { get; private set; }

        public const string QUESTION_MUST_BE_NOT_BLANK = "Question must be not blank";

        private readonly ICliSharpConsole cliSharpConsole;
        private readonly bool printLineNumber;

        public CliSharpView(
            ICliSharpConsole cliSharpConsole,
            CliSharpData cliSharpData,
            bool printLineNumber = false)
        {
            this.cliSharpConsole = cliSharpConsole;
            Data = cliSharpData;
            this.printLineNumber = printLineNumber;
        }

        public string AskFor(string question, bool sensitive = false)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new ArgumentException(QUESTION_MUST_BE_NOT_BLANK, nameof(question));

            Print($"{question}:", false, true);

            return sensitive ? cliSharpConsole.ReadSensitive() : cliSharpConsole.ReadLine();
        }

        public bool Confirm(string question = "Do you agree?", string yes = "Y", string no = "n")
        {
            return string.Equals(AskFor($"{question} ({yes}/{no})"), yes, StringComparison.CurrentCultureIgnoreCase);
        }

        public void PrintEmpty()
        {
            Print("");
        }

        public void Print(string text, bool emptyLineAfterPrint = false, bool noBreak = false)
        {
            PrintedLines++;

            if (printLineNumber)
            {
                if (noBreak)
                    cliSharpConsole.Write(text, PrintedLines);
                else
                    cliSharpConsole.WriteLine(text, PrintedLines);
            }
            else
            {
                if (noBreak)
                    cliSharpConsole.Write(text);
                else
                    cliSharpConsole.WriteLine(text);
            }

            if (emptyLineAfterPrint)
                PrintEmpty();
        }

        public string AskForSensitive(string question)
        {
            return AskFor(question, true);
        }

        private void PrintTitle()
        {
            PrintEmpty();
            Print($"{Data.Title}. Version: {Data.Version}", true);
        }

        public void PrintHelp(ICliSharpCommand command, Exception exception)
        {
            PrintException(exception);
            PrintHelp(command);
        }

        public void PrintHelp(ICliSharpCommand command)
        {
            PrintTitle();
            PrintCommand(command);
        }

        private void PrintException(Exception exception)
        {
            PrintEmpty();
            PrintSeparator();
            PrintEmpty();
            Print($"Error: {exception.GetType().Name}: {exception.Message}");
            PrintEmpty();
            PrintSeparator();
        }

        public void PrintSeparator()
        {
            Print("-----------#-----------");
        }

        private void PrintCommand(ICliSharpCommand command)
        {
            bool hasCommands = command.HasCommands();

            PrintHeader(command, hasCommands);
            PrintOptions(command);

            if (hasCommands)
            {
                PrintChildrenCommands(command);
            }
        }

        private void PrintChildrenCommands(ICliSharpCommand command)
        {
            Print("[commands]:", true);

            foreach (var item in command.Commands.OrderBy(obj => obj.Key)
                         .ToDictionary(obj => obj.Key, obj => obj.Value))
            {
                if (item.Key != command.Id)
                {
                    Print("".PadRight(3) + $"{item.Key,-39}{item.Value.Description}");
                }
            }

            PrintEmpty();
        }

        private void PrintOptions(ICliSharpCommand command)
        {
            Print("[options]:", true);
            Print(
                "".PadRight(3) + "Shortcut".PadRight(11) + "Option".PadRight(28) + "Description".PadRight(55) +
                "Parameters: (R)equired | (O)ptional = Length", true);

            foreach (var item in command.AvailableOptions.Itens.OrderBy(x => x.Key))
            {
                string paramsText = item.Value.Parameters.ToString();

                Print("".PadRight(2) +
                      $"{(item.Value.Shortcut == null ? "" : "-" + item.Value.Shortcut),-10}--{item.Key,-28}{item.Value.Description,-55}{paramsText}");
            }

            PrintEmpty();
        }

        private void PrintHeader(ICliSharpCommand command, bool hasCommands)
        {
            ICliSharpCommand? parent = command.Parent;
            string parentCommands = "";

            while (parent != null)
            {
                parentCommands = parent.Id + " " + parentCommands;
                parent = parent.Parent;
            }

            Print($"Usage: {parentCommands}{command.Id} [options]{(hasCommands ? " [commands]" : "")}");
            PrintEmpty();
            Print(command.Description, true);
        }
    }
}