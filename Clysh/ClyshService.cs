using System;
using System.Collections.Generic;
using System.Linq;
using Clysh.Data;

namespace Clysh
{
    public class ClyshService : IClyshService
    {
        private readonly ClyshSetup setup;
        public IClyshCommand RootCommand { get; private set; }
        public IClyshView View { get; }

        public ClyshService(ClyshSetup setup)
        {
            this.setup = setup;
            RootCommand = setup.RootCommand;
            View = new ClyshView(new ClyshConsole(), setup.Data);
        }
        
        public ClyshService(ClyshSetup setup, IClyshConsole clyshConsole)
        {
            this.setup = setup;
            RootCommand = setup.RootCommand;
            View = new ClyshView(clyshConsole, setup.Data);
        }

        public ClyshService(IClyshCommand rootCommand, IClyshView view)
        {
            RootCommand = rootCommand;
            View = view;
        }

        public void Execute(string[] args)
        {
            ClyshOption? lastOption = null;
            IClyshCommand lastCommand = RootCommand;
            bool isOptionHelp = false;

            RootCommand.Order = 0;
            List<IClyshCommand> commandsToExecute = new()
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
            catch (Exception e)
            {
                ExecuteHelp(lastCommand, e);
            }

        }

        private static ClyshOption GetOptionFromCommand(IClyshCommand lastCommand, string arg)
        {
            ClyshOption? lastOption;
            string key = ArgIsOptionFull(arg) ? arg[2..] : arg[1..];

            if (!lastCommand.HasOption(key))
                throw new InvalidOperationException($"The option '{arg}' is invalid.");

            lastOption = lastCommand.GetOption(key);
            return lastOption;
        }

        private static void ProcessParameter(ClyshOption? lastOption, string arg)
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

        private static void CheckLastOptionStatus(ClyshOption? lastOption)
        {
            if (lastOption != null && lastOption.Parameters.WaitingForRequired())
                ThrowRequiredParametersError(lastOption);
        }

        private static void ThrowRequiredParametersError(ClyshOption lastOption)
        {
            throw new InvalidOperationException($"Required parameters [{lastOption.Parameters.RequiredToString()}] is missing for option: {lastOption.Id}");
        }

        private static bool ArgIsParameter(string arg)
        {
            return arg.Contains(':');
        }

        private void Execute(List<IClyshCommand> commandsToExecute)
        {
            foreach (IClyshCommand command in commandsToExecute.OrderBy(x => x.Order))
            {
                if (command.Action == null)
                    throw new ArgumentNullException(nameof(commandsToExecute), "Action null");

                command.Action(command.SelectedOptions, View);
            }

        }

        private static IClyshCommand GetCommandFromArg(IClyshCommand lastCommand, string arg)
        {
            int order = lastCommand.Order + 1;
            lastCommand = lastCommand.GetCommand(arg);
            lastCommand.Order = order;
            return lastCommand;
        }

        public void ExecuteHelp(IClyshCommand command, Exception exception)
        {
            View.PrintHelp(command, exception);
        }

        public void ExecuteHelp(IClyshCommand command)
        {
            View.PrintHelp(command);
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