using CliSharp.Data;

namespace CliSharp
{
    public class CliSharpService : ICliSharpService
    {
        public ICliSharpCommand RootCommand { get; private set; }
        public ICliSharpView Front { get; }

        public CliSharpService(
            ICliSharpCommand rootCommand,
            ICliSharpConsole cliSharpConsole,
            CliSharpData cliSharpData)
        {
            RootCommand = rootCommand;
            Front = new CliSharpView(cliSharpConsole, cliSharpData);
        }

        public CliSharpService(
            ICliSharpCommand rootCommand,
            ICliSharpView front)
        {
            RootCommand = rootCommand;
            Front = front;
        }

        public void Execute(string[] args)
        {
            CliSharpOption? lastOption = null;
            ICliSharpCommand lastCommand = RootCommand;
            bool isOptionHelp = false;

            RootCommand.Order = 0;
            List<ICliSharpCommand> commandsToExecute = new()
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

        private static CliSharpOption GetOptionFromCommand(ICliSharpCommand lastCommand, string arg)
        {
            CliSharpOption? lastOption;
            string key = ArgIsOptionFull(arg) ? arg[2..] : arg[1..];

            if (!lastCommand.HasOption(key))
                throw new InvalidOperationException($"The option '{arg}' is invalid.");

            lastOption = lastCommand.GetOption(key);
            return lastOption;
        }

        private static void ProcessParameter(CliSharpOption? lastOption, string arg)
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

        private static void CheckLastOptionStatus(CliSharpOption? lastOption)
        {
            if (lastOption != null && lastOption.Parameters.WaitingForRequired())
                ThrowRequiredParametersError(lastOption);
        }

        private static void ThrowRequiredParametersError(CliSharpOption lastOption)
        {
            throw new InvalidOperationException($"Required parameters [{lastOption.Parameters.RequiredToString()}] is missing for option: {lastOption.Id}");
        }

        private static bool ArgIsParameter(string arg)
        {
            return arg.Contains(':');
        }

        private void Execute(List<ICliSharpCommand> commandsToExecute)
        {
            foreach (ICliSharpCommand command in commandsToExecute.OrderBy(x => x.Order))
            {
                if (command.Action == null)
                    throw new ArgumentNullException(nameof(commandsToExecute), "Action null");

                command.Action(command.SelectedOptions, this.Front);
            }

        }

        private static ICliSharpCommand GetCommandFromArg(ICliSharpCommand lastCommand, string arg)
        {
            int order = lastCommand.Order + 1;
            lastCommand = lastCommand.GetCommand(arg);
            lastCommand.Order = order;
            return lastCommand;
        }

        public void ExecuteHelp(ICliSharpCommand command, Exception exception)
        {
            Front.PrintHelp(command, exception);
        }

        public void ExecuteHelp(ICliSharpCommand command)
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