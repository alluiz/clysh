namespace CommandLineInterface
{
    public interface ICommandLineInterface
    {
        string Title { get; set; }
        ICommand RootCommand { get; }
        string? AskFor(string text);
        bool Confirm(string text = "Do you agree? (Y/n): ", string yes = "Y");
        ICommand CreateCommand(string name, string description, Action<ICommand, Options, ICommandLineInterface> action);
        void EmptyLine();
        void Execute(string[] args);
        void ExecuteHelp(ICommand command, Exception? exception);
        void ExecuteHelp(ICommand command);
        void Show(string text, bool emptyLine = false);
        void ShowNoBreak(string text);
    }

    public class CommandLineInterface : ICommandLineInterface
    {
        public string Title { get; set; }
        public ICommand RootCommand { get; private set; }

        private readonly IConsoleManager console;

        public CommandLineInterface(IConsoleManager consoleManager, Metadata metadata, Action<ICommand, Options, ICommandLineInterface> defaultAction)
        {
            console = consoleManager;
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

                                        while (waitingForArguments > 0)
                                        {
                                            lastOption.Arguments[argumentIndex].Value = args[i + 1];
                                            argumentIndex++;
                                            waitingForArguments--;
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
                            lastCommand = GetCommandFromArg(lastCommand, arg);
                            commandsToExecute.Add(lastCommand);
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

        public string? AskFor(string text)
        {
            ShowNoBreak(text);
            return console.ReadLine();
        }

        public bool Confirm(string text = "Do you agree? (Y/n): ", string yes = "Y")
        {
            return AskFor(text)?.ToUpper() == yes.ToUpper();
        }

        public void ShowNoBreak(string text)
        {
            console.Write(text);
        }

        public void ExecuteHelp(ICommand command, Exception? exception)
        {
            if (exception != null)
            {
                Show(exception.ToString());
                console.Separator();
            }

            EmptyLine();
            Show(Title, true);

            command.ActionHelp(this);
        }

        public void ExecuteHelp(ICommand command)
        {
            ExecuteHelp(command, null);
        }

        public void EmptyLine()
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

        public void Show(string text, bool emptyLine = false)
        {
            console.WriteLine(text);

            if (emptyLine)
                EmptyLine();
        }
    }
}