namespace CommandLineInterface
{
    public class CommandLineInterface : ICommandLineInterface
    {
        public ICommand RootCommand { get; private set; }
        public ICommandLineInterfaceFront Front { get; }

        public CommandLineInterface(
            ICommand rootCommand,
            IConsoleManager console,
            Metadata metadata)
        {
            RootCommand = rootCommand;
            Front = new CommandLineInterfaceFront(console, metadata);
        }

        public CommandLineInterface(
            ICommand rootCommand,
            ICommandLineInterfaceFront front)
        {
            RootCommand = rootCommand;
            Front = front;
        }

        public void Execute(string[] args)
        {
            Option? lastOption = null;
            ICommand lastCommand = RootCommand;
            bool isOptionHelp = false;

            RootCommand.Order = 0;
            List<ICommand> commandsToExecute = new()
            {
                RootCommand
            };

            try
            {
                for (int i = 0; i < args.Length && !isOptionHelp; i++)
                {
                    string arg = args[i];

                    if (ArgIsOption(arg))
                    {
                        CheckLastOptionStatus(lastOption);
                        lastOption = GetOptionFromCommand(lastCommand, arg);
                        lastCommand.AddSelectedOption(lastOption);
                        isOptionHelp = lastOption.Id.Equals("help");
                    }
                    else if (lastCommand.HasCommand(arg))
                    {
                        CheckLastOptionStatus(lastOption);
                        lastOption = null;
                        lastCommand = GetCommandFromArg(lastCommand, arg);
                        commandsToExecute.Add(lastCommand);
                    }
                    else if (string.IsNullOrEmpty(arg) || string.IsNullOrWhiteSpace(arg))
                        continue;
                    else
                        ProcessParameter(lastOption, arg);
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

        private static Option GetOptionFromCommand(ICommand lastCommand, string arg)
        {
            Option? lastOption;
            string key = ArgIsOptionFull(arg) ? arg[2..] : arg[1..];

            if (!lastCommand.HasOption(key))
                throw new InvalidOperationException($"The option '{arg}' is invalid.");

            lastOption = lastCommand.GetOption(key);
            return lastOption;
        }

        private static void ProcessParameter(Option? lastOption, string arg)
        {
            if (lastOption == null)
                throw new InvalidOperationException("You can't put parameters without any option that accept it.");

            if (ArgIsParameter(arg))
            {
                string[] parameter = arg.Split(":");

                string id = parameter[0];
                string data = parameter[1];

                if (lastOption.Parameters.Has(id))
                {
                    if (lastOption.Parameters.Get(id).Data != null)
                        throw new InvalidOperationException($"The parameter '{id}' is already filled for option: {lastOption.Id}.");

                    lastOption.Parameters.Get(id).Data = data;
                }
                else
                    throw new InvalidOperationException($"The parameter '{id}' is invalid for option: {lastOption.Id}.");
            }
            else
            {
                if (!lastOption.Parameters.WaitingForAny())
                    throw new InvalidOperationException($"The parameter data '{arg}' is out of bound for option: {lastOption.Id}.");

                lastOption.Parameters.Last().Data = arg;
            }
        }

        private static void CheckLastOptionStatus(Option? lastOption)
        {
            if (lastOption != null && lastOption.Parameters.WaitingForRequired())
                ThrowRequiredParametersError(lastOption);
        }

        private static void ThrowRequiredParametersError(Option lastOption)
        {
            throw new InvalidOperationException($"Required parameters [{lastOption.Parameters.RequiredToString()}] is missing for option: {lastOption.Id}");
        }

        private static bool ArgIsParameter(string arg)
        {
            return arg.Contains(':');
        }

        private void Execute(List<ICommand> commandsToExecute)
        {
            foreach (ICommand command in commandsToExecute.OrderBy(x => x.Order))
            {
                if (command.Action == null)
                    throw new ArgumentNullException(nameof(commandsToExecute), "Action null");

                command.Action(command.SelectedOptions, this.Front);
            }

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

        private static bool ArgIsOption(string arg)
        {
            return arg.StartsWith("-");
        }

        private static bool ArgIsOptionFull(string arg)
        {
            return arg.StartsWith("--");
        }
    }
}