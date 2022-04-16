namespace CommandLineInterface
{
    public class CommandLineInterface : ICommandLineInterface
    {
        public ICommand RootCommand { get; private set; }
        public ICommandLineInterfaceFront Front { get; }

        public CommandLineInterface(
            Action<Map<Option>, ICommandLineInterfaceFront> defaultAction!!,
            IConsoleManager console!!,
            Metadata metadata!!)
        {
            RootCommand = CreateRootCommand(defaultAction);
            Front = new CommandLineInterfaceFront(console, metadata);
        }

        public CommandLineInterface(
            Action<Map<Option>, ICommandLineInterfaceFront> defaultAction!!,
            ICommandLineInterfaceFront front!!)
        {
            RootCommand = CreateRootCommand(defaultAction);
            Front = front;
        }

        public CommandLineInterface(
            ICommand rootCommand!!,
            IConsoleManager console!!,
            Metadata metadata!!)
        {
            RootCommand = rootCommand;
            Front = new CommandLineInterfaceFront(console, metadata);
        }

        public CommandLineInterface(
            ICommand rootCommand!!,
            ICommandLineInterfaceFront front!!)
        {
            RootCommand = rootCommand;
            Front = front;
        }


        public ICommand CreateCommand(string name, string description, Action<Map<Option>, ICommandLineInterfaceFront> action)
        {
            return new Command(name, description, action);
        }

        protected ICommand CreateRootCommand(Action<Map<Option>, ICommandLineInterfaceFront> action)
        {
            return CreateCommand("root", "The root command (default)", action);
        }

        public void Execute(string[] args)
        {
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

                    if (ArgIsArgument(arg))
                    {
                        ProcessArgument(lastOption, arg);
                    }
                    else
                    {
                        CheckLastOptionStatus(lastOption);

                        if (ArgIsOption(arg))
                        {

                            if (IsMultiOption(arg))
                            {
                                for (int j = 1; j < arg.Length; j++)
                                {
                                    lastOption = GetOptionFromCommand(lastCommand, arg[j].ToString());

                                    try
                                    {
                                        while (lastOption.Arguments.Waiting())
                                        {
                                            if (!ArgIsArgument(arg))
                                                throwRequiredArgumentsError(lastOption);

                                            ProcessArgument(lastOption, args[i + 1]);
                                        }
                                    }
                                    catch (System.IndexOutOfRangeException)
                                    {
                                        throwRequiredArgumentsError(lastOption);
                                    }

                                    lastCommand.AddSelectedOption(lastOption);
                                }
                            }
                            else
                            {
                                lastOption = GetOptionFromCommand(lastCommand, arg);
                                lastCommand.AddSelectedOption(lastOption);
                                isOptionHelp = lastOption.Id.Equals("help");
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

                CheckLastOptionStatus(lastOption);

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

        private Option GetOptionFromCommand(ICommand lastCommand, string arg)
        {
            Option? lastOption;
            string key = ArgIsOptionFull(arg) ? arg.Substring(2) : ArgIsOption(arg) ? arg.Substring(1) : arg;

            if (!lastCommand.HasOption(key))
                throw new InvalidOperationException($"The option '{arg}' is invalid.");

            lastOption = lastCommand.GetOption(key);
            return lastOption;
        }

        private bool IsMultiOption(string arg)
        {
            return arg.Length > 2 && !ArgIsOptionFull(arg);
        }

        private void ProcessArgument(Option? lastOption, string arg)
        {
            if (lastOption == null)
                throw new InvalidOperationException($"You can't put arguments without any option");

            string[] argument = arg.Split(":");
            string id = argument[0];
            string value = argument[1];

            if (lastOption.Arguments.Required.Has(id))
                lastOption.Arguments.Required.Get(id).Value = value;
            else if (lastOption.Arguments.Optional.Has(id))
                lastOption.Arguments.Optional.Get(id).Value = value;
            else
                throw new InvalidOperationException($"The argument '{arg}' is invalid for option: {lastOption.Id}.");
        }

        private void CheckLastOptionStatus(Option? lastOption)
        {
            if (lastOption != null && lastOption.Arguments.Waiting())
                throwRequiredArgumentsError(lastOption);
        }

        private void throwRequiredArgumentsError(Option lastOption)
        {
            throw new InvalidOperationException($"Required arguments [{lastOption.Arguments.Required.ToString()}] is missing for option: {lastOption.Id}");
        }

        private bool ArgIsArgument(string arg)
        {
            return arg.Contains(":");
        }

        private void Execute(List<ICommand> commandsToExecute)
        {
            foreach (ICommand command in commandsToExecute.OrderBy(x => x.Order))
                command.Action(command.SelectedOptions, this.Front);
        }

        private static ICommand GetCommandFromArg(ICommand lastCommand, string arg)
        {
            int order = lastCommand.Order + 1;
            lastCommand = lastCommand.GetCommand(arg);
            lastCommand.Order = order;
            return lastCommand;
        }

        public void ExecuteHelp(ICommand command, Exception exception)
        {
            Front.PrintHelp(command, exception);
        }

        public void ExecuteHelp(ICommand command)
        {
            Front.PrintHelp(command);
        }

        private bool ArgIsOption(string arg)
        {
            return arg.StartsWith("-");
        }

        private bool ArgIsOptionFull(string arg)
        {
            return arg.StartsWith("--");
        }
    }
}