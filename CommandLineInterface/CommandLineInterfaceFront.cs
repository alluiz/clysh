namespace CommandLineInterface
{
    public class CommandLineInterfaceFront : ICommandLineInterfaceFront
    {
        public Metadata Metadata { get; set; }
        public const string QUESTION_MUST_BE_NOT_BLANK = "Question must be not blank";

        private readonly IConsoleManager console;

        public CommandLineInterfaceFront(
            IConsoleManager console!!,
            Metadata metadata!!)
        {
            this.console = console;
            Metadata = metadata;
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
            console.Write(text);
        }

        public void PrintEmptyLine()
        {
            console.EmptyLine();
        }
        public void PrintWithBreak(string text, bool emptyLineAfterPrint = false)
        {
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
            PrintTitle();
            PrintHelp(command);
        }

        public void PrintHelp(ICommand command)
        {
            PrintTitle();
            PrintCommand(command);
        }

        private void PrintException(Exception exception)
        {
            PrintWithBreak($"{exception.GetType().Name}: {exception.Message}");
            PrintSeparator();
        }

        public void PrintSeparator()
        {
            console.Separator();
        }

        private void PrintCommand(ICommand command)
        {
            ICommand? parent = command.Parent;
            string parentCommands = "";

            while (parent != null)
            {
                parentCommands += parent.Name + " ";
                parent = parent.Parent;
            }

            bool hasCommands = command.HasCommands();

            this.PrintWithBreak($"Usage: {parentCommands}{command.Name} [options] {(hasCommands ? "[commands]" : "")}");
            this.PrintEmptyLine();
            this.PrintWithBreak(command.Description, true);
            this.PrintWithBreak("[options]:".PadRight(44) + "Description".PadRight(55) + "Arguments", true);

            foreach (var item in command.AvailableOptions.Itens.OrderBy(x => x.Key))
            {
                string args = "";

                foreach (var argument in item.Value.Arguments.Required.Itens)
                {
                    args += $"<{argument.Value.Id}:Required>";
                }

                foreach (var argument in item.Value.Arguments.Optional.Itens)
                {
                    args += $"<{argument.Value.Id}:Required>";
                }


                this.PrintWithBreak($"    {(item.Value.Abbreviation == null ? "" : "-" + item.Value.Abbreviation).PadRight(10)}--{item.Key.PadRight(28)}{item.Value.Description.PadRight(55)}{args}");
            }

            this.PrintEmptyLine();

            if (hasCommands)
            {
                this.PrintWithBreak("[commands]:", true);

                foreach (var item in command.Commands.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value))
                {
                    if (item.Key != command.Name)
                    {
                        this.PrintWithBreak($"    {item.Key.PadRight(40)}{item.Value.Description}");
                    }
                }

                this.PrintEmptyLine();
            }
        }
    }
}