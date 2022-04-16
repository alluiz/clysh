namespace CommandLineInterface
{
    public class CommandLineInterfaceFront : ICommandLineInterfaceFront
    {
        public Metadata Metadata { get; set; }
        public int PrintedLines { get; private set; }

        public const string QUESTION_MUST_BE_NOT_BLANK = "Question must be not blank";

        private readonly IConsoleManager console;
        private readonly bool printLineNumber;

        public CommandLineInterfaceFront(
            IConsoleManager console!!,
            Metadata metadata!!,
            bool printLineNumber = false)
        {
            this.console = console;
            Metadata = metadata;
            this.printLineNumber = printLineNumber;
        }

        public string AskFor(string question!!, bool sensitive = false)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new ArgumentException(QUESTION_MUST_BE_NOT_BLANK, nameof(question));

            Print($"{question}:");
            return sensitive ? console.ReadSensitive() : console.ReadLine();
        }

        public bool Confirm(string question = "Do you agree?", string yes = "Y")
        {
            return AskFor($"{question} ({yes}/n)").ToUpper() == yes.ToUpper();
        }

        public void Print(string text)
        {
            PrintedLines++;

            if (printLineNumber)
                console.Write(text, PrintedLines);
            else
                console.Write(text);
        }

        public void PrintEmptyLine()
        {
            PrintWithBreak("");
        }

        public void PrintWithBreak(string text, bool emptyLineAfterPrint = false)
        {
            PrintedLines++;

            if (printLineNumber)
                console.WriteLine(text, PrintedLines);
            else
                console.WriteLine(text);

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
            PrintWithBreak(Metadata.Title, true);
        }

        public void PrintHelp(ICommand command, Exception exception)
        {
            PrintException(exception);
            PrintHelp(command);
        }

        public void PrintHelp(ICommand command)
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

        private void PrintCommand(ICommand command)
        {
            bool hasCommands = command.HasCommands();

            PrintHeader(command, hasCommands);
            PrintOptions(command);

            if (hasCommands)
            {
                PrintChildrenCommands(command);
            }
        }

        private void PrintChildrenCommands(ICommand command)
        {
            this.PrintWithBreak("[commands]:", true);

            foreach (var item in command.Commands.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value))
            {
                if (item.Key != command.Name)
                {
                    this.PrintWithBreak($"   {item.Key,-39}{item.Value.Description}");
                }
            }

            this.PrintEmptyLine();
        }

        private void PrintOptions(ICommand command)
        {
            this.PrintWithBreak("[options]:", true);
            this.PrintWithBreak("   Abbrev.".PadRight(14) + "Option".PadRight(28) + "Description".PadRight(55) + "Parameters", true);

            foreach (var item in command.AvailableOptions.Itens.OrderBy(x => x.Key))
            {
                string args = "";

                foreach (var parameter in item.Value.Parameters.Itens.ToList().Where(x => x != null).OrderBy(x => x.Required).ThenBy(x => x.Id))
                {
                    string type = parameter.Required ? "Required" : "Optional";
                    args += $"<{parameter.Id}:{type}>";
                }

                this.PrintWithBreak($"  {(item.Value.Abbreviation == null ? "" : "-" + item.Value.Abbreviation),-10}--{item.Key,-28}{item.Value.Description,-55}{args}");
            }

            this.PrintEmptyLine();
        }

        private void PrintHeader(ICommand command, bool hasCommands)
        {
            ICommand? parent = command.Parent;
            string parentCommands = "";

            while (parent != null)
            {
                parentCommands += parent.Name + " ";
                parent = parent.Parent;
            }

            this.PrintWithBreak($"Usage: {parentCommands}{command.Name} [options] {(hasCommands ? "[commands]" : "")}");
            this.PrintEmptyLine();
            this.PrintWithBreak(command.Description, true);
        }
    }
}