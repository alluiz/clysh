namespace CommandLineInterface
{
    public class CommandLineInterface : ICommandLineInterface
    {
        public ICommand RootCommand { get; private set; }
        public ICommandLineInterfaceFront Front { get; }

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

                    if (ArgIsParameter(arg))
                    {
                        ProcessParameter(lastOption, arg);
                    }
                    else
                    {
                        CheckLastOptionStatus(lastOption);

                        if (ArgIsOption(arg))
                        {

                            if (IsMultiOption(arg))
                            {
                                processMultiOption(lastCommand, arg);
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

        private void processMultiOption(ICommand lastCommand, string arg)
        {
            for (int j = 1; j < arg.Length; j++)
            {
                Option lastOption = GetOptionFromCommand(lastCommand, arg[j].ToString());
                lastCommand.AddSelectedOption(lastOption);
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

        private void ProcessParameter(Option? lastOption, string arg)
        {
            if (lastOption == null)
                throw new InvalidOperationException("You can't put parameters without any option that accept it.");

            string[] parameter = arg.Split(":");
            string id = parameter[0];
            string value = parameter[1];

            if (lastOption.Parameters.Required.Has(id))
                lastOption.Parameters.Required.Get(id).Data = value;
            else if (lastOption.Parameters.Optional.Has(id))
                lastOption.Parameters.Optional.Get(id).Data = value;
            else
                throw new InvalidOperationException($"The parameter '{arg}' is invalid for option: {lastOption.Id}.");
        }

        private void CheckLastOptionStatus(Option? lastOption)
        {
            if (lastOption != null && lastOption.Parameters.Waiting())
                throwRequiredParametersError(lastOption);
        }

        private void throwRequiredParametersError(Option lastOption)
        {
            throw new InvalidOperationException($"Required parameters [{lastOption.Parameters.Required.ToString()}] is missing for option: {lastOption.Id}");
        }

        private bool ArgIsParameter(string arg)
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