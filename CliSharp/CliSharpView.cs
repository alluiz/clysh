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

            Print($"{question}:");
            return sensitive ? cliSharpConsole.ReadSensitive() : cliSharpConsole.ReadLine();
        }

        public bool Confirm(string question = "Do you agree?", string yes = "Y", string no = "n")
        {
            return AskFor($"{question} ({yes}/{no})").ToUpper() == yes.ToUpper();
        }

        public void Print(string text)
        {
            PrintedLines++;

            if (printLineNumber)
                cliSharpConsole.Write(text, PrintedLines);
            else
                cliSharpConsole.Write(text);
        }

        public void PrintEmptyLine()
        {
            PrintWithBreak("");
        }

        public void PrintWithBreak(string text, bool emptyLineAfterPrint = false)
        {
            PrintedLines++;

            if (printLineNumber)
                cliSharpConsole.WriteLine(text, PrintedLines);
            else
                cliSharpConsole.WriteLine(text);

            if (emptyLineAfterPrint)
                PrintEmptyLine();
        }

        public string AskForSensitive(string question)
        {
            return AskFor(question, true);
        }

        private void PrintTitle()
        {
            PrintEmptyLine();
            PrintWithBreak(Data.Title, true);
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
            PrintEmptyLine();
            PrintSeparator();
            PrintEmptyLine();
            PrintWithBreak($"Error: {exception.GetType().Name}: {exception.Message}");
            PrintEmptyLine();
            PrintSeparator();
        }

        public void PrintSeparator()
        {
            PrintWithBreak("-----------#-----------");
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
            this.PrintWithBreak("[commands]:", true);

            foreach (var item in command.Commands.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value))
            {
                if (item.Key != command.Id)
                {
                    this.PrintWithBreak("".PadRight(3) + $"{item.Key,-39}{item.Value.Description}");
                }
            }

            this.PrintEmptyLine();
        }

        private void PrintOptions(ICliSharpCommand command)
        {
            this.PrintWithBreak("[options]:", true);
            this.PrintWithBreak("".PadRight(3) + "Abbrev.".PadRight(11) + "Option".PadRight(28) + "Description".PadRight(55) + "Parameters: (R)equired | (O)ptional = Length", true);

            foreach (var item in command.AvailableOptions.Itens.OrderBy(x => x.Key))
            {
                string paramsText = item.Value.Parameters.ToString();

                this.PrintWithBreak("".PadRight(2) + $"{(item.Value.Abbreviation == null ? "" : "-" + item.Value.Abbreviation),-10}--{item.Key,-28}{item.Value.Description,-55}{paramsText}");
            }

            this.PrintEmptyLine();
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

            this.PrintWithBreak($"Usage: {parentCommands}{command.Id} [options]{(hasCommands ? " [commands]" : "")}");
            this.PrintEmptyLine();
            this.PrintWithBreak(command.Description, true);
        }
    }
}