namespace CommandLineInterface
{
    public class CommandLineInterface : ICommandLineInterface
    {
        public string Title { get; set; }
        public ICommand RootCommand { get; private set; }

        public const string QUESTION_MUST_BE_NOT_BLANK = "Question must be not blank";

        private readonly IConsoleManager console;

        public CommandLineInterface(
            IConsoleManager console!!,
            Metadata metadata!!,
            Action<ICommand, Options, ICommandLineInterface> defaultAction!!)
        {
            this.console = console;
            Title = metadata.Title;

            RootCommand = CreateRootCommand(defaultAction, metadata);
        }

        public ICommand CreateCommand(string name, string description, Action<ICommand, Options, ICommandLineInterface> action)
        {
            return new Command(name, description, action);
        }

        protected ICommand CreateRootCommand(Action<ICommand, Options, ICommandLineInterface> action, Metadata metadata)
        {
            return CreateCommand(metadata.RootCommandName, metadata.Description, action);
        }

        public void Execute(string[] args)
        {
            int waitingForArguments = 0;
            int argumentIndex = 0;

            Option? lastOption = null;
            ICommand lastCommand = RootCommand;
            bool isOptionHelp = false;

            RootCommand.Order = 0;
            List<ICommand> commandsToExecute = new List<ICommand>();
            commandsToExecute.Add(RootCommand);

            try
            {
                for (int i = 0; i < args.Length && !isOptionHelp; i++)
                {
                    string arg = args[i];

                    if (waitingForArguments > 0)
                    {
                        if (lastOption != null && lastOption.Arguments != null)
                            lastOption.Arguments[argumentIndex].Value = arg;

                        waitingForArguments--;
                        argumentIndex++;
                    }
                    else
                    {
                        if (ArgIsOption(arg))
                        {
                            if (arg.Length > 2 && !ArgIsOptionFull(arg))
                            {
                                for (int j = 1; j < arg.Length; j++)
                                {
                                    string option = arg[j].ToString();

                                    lastOption = lastCommand.GetOption(option);

                                    if (lastOption.Arguments.Length > 0)
                                    {
                                        argumentIndex = 0;
                                        waitingForArguments = lastOption.Arguments.Length;

                                        try
                                        {
                                            while (waitingForArguments > 0)
                                            {
                                                lastOption.Arguments[argumentIndex].Value = args[i + 1];
                                                argumentIndex++;
                                                waitingForArguments--;
                                            }

                                        }
                                        catch (System.IndexOutOfRangeException)
                                        {
                                            throw new InvalidOperationException($"Required argument index {lastOption.Arguments?.Length - waitingForArguments} missing for option: {lastOption.Name}");
                                        }
                                    }

                                    lastCommand.AddSelectedOption(lastOption);
                                }
                            }
                            else
                            {
                                string key = ArgIsOptionFull(arg) ? arg.Substring(2) : arg.Substring(1);

                                if (!lastCommand.HasOption(key))
                                    throw new InvalidOperationException($"The option '{arg}' is invalid.");

                                lastOption = lastCommand.GetOption(key);

                                lastCommand.AddSelectedOption(lastOption);
                                isOptionHelp = lastOption.Name.Equals("help");

                                if (lastOption.Arguments.Length > 0)
                                {
                                    argumentIndex = 0;
                                    waitingForArguments = lastOption.Arguments.Length;
                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(arg) && !string.IsNullOrWhiteSpace(arg))
                            {
                                lastCommand = GetCommandFromArg(lastCommand, arg);
                                commandsToExecute.Add(lastCommand);
                            }
                        }
                    }
                }

                if (waitingForArguments > 0 && lastOption != null)
                    throw new InvalidOperationException($"Required argument index {lastOption.Arguments?.Length - waitingForArguments} missing for option: {lastOption.Name}");


                if (isOptionHelp)
                    ExecuteHelp(lastCommand);
                else
                    Execute(commandsToExecute);

            }
            catch (System.Exception e)
            {
                ExecuteHelp(lastCommand, e);
            }

        }

        private void Execute(List<ICommand> commandsToExecute)
        {
            foreach (ICommand command in commandsToExecute.OrderBy(x => x.Order))
                command.Action(command, command.SelectedOptions, this);
        }

        private static ICommand GetCommandFromArg(ICommand lastCommand, string arg)
        {
            int order = lastCommand.Order + 1;
            lastCommand = lastCommand.GetCommand(arg);
            lastCommand.Order = order;
            return lastCommand;
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

        public void ExecuteHelp(ICommand command, Exception? exception)
        {
            if (exception != null)
            {
                PrintWithBreak(exception.ToString());
                console.Separator();
            }

            PrintEmptyLine();
            PrintWithBreak(Title, true);

            command.ActionHelp(this);
        }

        public void ExecuteHelp(ICommand command)
        {
            ExecuteHelp(command, null);
        }

        public void PrintEmptyLine()
        {
            console.EmptyLine();
        }

        private bool ArgIsOption(string arg)
        {
            return arg.StartsWith("-");
        }

        private bool ArgIsOptionFull(string arg)
        {
            return arg.StartsWith("--");
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
    }
}