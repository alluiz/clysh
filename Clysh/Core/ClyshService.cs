using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Clysh.Core;

public class ClyshService : IClyshService
{
    public IClyshCommand RootCommand { get; private set; }
    public IClyshView View { get; }

    [ExcludeFromCodeCoverage]
    public ClyshService(ClyshSetup setup, bool disableSafeMode = false) : this(setup, new ClyshConsole(),
        disableSafeMode)
    {
    }

    [ExcludeFromCodeCoverage]
    private ClyshService(ClyshSetup setup, IClyshConsole clyshConsole, bool disableSafeMode = false)
    {
        if (!disableSafeMode && !setup.IsReadyToProduction())
            throw new ClyshException(
                "Your CLI are not ready to production. Check if ALL of your commands has a configured action and a valid description.");

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
        var lastCommand = RootCommand;
        var isOptionHelp = false;

        RootCommand.Order = 0;
        List<IClyshCommand> commandsToExecute = new()
        {
            RootCommand
        };

        try
        {
            for (var i = 0; i < args.Length && !isOptionHelp; i++)
            {
                var arg = args[i];

                if (ArgIsOption(arg))
                {
                    CheckLastOptionStatus(lastOption);
                    lastOption = GetOptionFromCommand(lastCommand, arg);

                    if (lastOption.Group != null)
                    {
                        var oldOptionOfGroupSelected = lastCommand
                            .GetOptionFromGroup(lastOption.Group.Id);

                        if (oldOptionOfGroupSelected != null)
                            oldOptionOfGroupSelected.Selected = false;
                    }

                    lastOption.Selected = true;
                    isOptionHelp = lastOption.Id.Equals("help");
                }
                else if (lastCommand.HasChild(arg))
                {
                    CheckLastOptionStatus(lastOption);
                    lastOption = null;
                    lastCommand.Executed = true;
                    lastCommand = GetCommandFromArg(lastCommand, arg);
                    commandsToExecute.Add(lastCommand);
                }
                else if (!string.IsNullOrEmpty(arg) && !string.IsNullOrWhiteSpace(arg))
                    ProcessParameter(lastOption, arg);
            }

            CheckLastCommandStatus(lastCommand);
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

    private void CheckLastCommandStatus(IClyshCommand lastCommand)
    {
        if (lastCommand.RequireSubcommand && !lastCommand.HasAnyChildrenExecuted())
            throw new InvalidOperationException($"You need to provide some subcommand to command '{lastCommand.Id}'");
    }

    private static ClyshOption GetOptionFromCommand(IClyshCommand lastCommand, string arg)
    {
        ClyshOption? lastOption;
        var key = ArgIsOptionFull(arg) ? arg[2..] : arg[1..];

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
            var parameter = arg.Split(":");

            var id = parameter[0];
            var data = parameter[1];

            if (lastOption.Parameters.Has(id))
            {
                if (lastOption.Parameters[id].Data != null)
                    throw new InvalidOperationException(
                        $"The parameter '{id}' is already filled for option: {lastOption.Id}.");

                lastOption.Parameters[id].Data = data;
            }
            else
                throw new InvalidOperationException(
                    $"The parameter '{id}' is invalid for option: {lastOption.Id}.");
        }
        else
        {
            if (!lastOption.Parameters.WaitingForAny())
                throw new InvalidOperationException(
                    $"The parameter data '{arg}' is out of bound for option: {lastOption.Id}.");

            lastOption.Parameters.Last().Data = arg;
        }
    }

    private static void CheckLastOptionStatus(ClyshOption? lastOption)
    {
        if (lastOption != null)
        {
            if (lastOption.Parameters.WaitingForRequired())
                ThrowRequiredParametersError(lastOption);
        }
    }

    private static void ThrowRequiredParametersError(ClyshOption lastOption)
    {
        throw new InvalidOperationException(
            $"Required parameters [{lastOption.Parameters.RequiredToString()}] is missing for option: {lastOption.Id} (shortcut: {lastOption.Shortcut ?? "<null>"})");
    }

    private static bool ArgIsParameter(string arg)
    {
        return arg.Contains(':');
    }

    private void Execute(List<IClyshCommand> commandsToExecute)
    {
        foreach (var command in commandsToExecute.OrderBy(x => x.Order))
        {
            if (command.Action != null)
            {
                command.Action(command, command.Options, View);
            }
            else if (!command.RequireSubcommand)
                throw new ArgumentNullException(nameof(commandsToExecute), "Action null");
        }
    }

    private static IClyshCommand GetCommandFromArg(IClyshCommand lastCommand, string arg)
    {
        var order = lastCommand.Order + 1;
        lastCommand = lastCommand.SubCommands[arg];
        lastCommand.Order = order;
        return lastCommand;
    }

    private void ExecuteHelp(IClyshCommand command, Exception exception)
    {
        View.PrintHelp(command, exception);
    }

    private void ExecuteHelp(IClyshCommand command)
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