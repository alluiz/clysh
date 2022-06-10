
using System.Diagnostics.CodeAnalysis;
using Clysh.Helper;

namespace Clysh.Core;

public class ClyshService : IClyshService
{
    public IClyshCommand RootCommand { get; }
    public IClyshView View { get; }
    public bool Completed { get; set; }

    private ClyshOption? lastOption;

    private IClyshCommand lastCommand;

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
        RootCommand.Order = 0;
        lastCommand = RootCommand;
        View = new ClyshView(clyshConsole, setup.Data);
    }

    public ClyshService(IClyshCommand rootCommand, IClyshView view)
    {
        RootCommand = rootCommand;
        RootCommand.Order = 0;
        lastCommand = RootCommand;
        View = view;
    }

    public void Execute(string[] args)
    {
        try
        {
            Completed = false;
            
            List<IClyshCommand> commandsToExecute = new() { lastCommand };
            
            foreach (var arg in args)
            {
                if (IsOption(arg))
                {
                    ProcessOption(arg);

                    if (OptionHelp())
                    {
                        ExecuteHelp();
                        break;
                    }
                }
                else
                {
                    if (IsSubcommand(arg))
                        ProcessSubcommand(arg, commandsToExecute);
                    else //is parameter
                        ProcessParameter(arg);
                }
            }

            if (!Completed)
            {
                CheckLastCommandStatus();
                CheckLastOptionStatus();
                Execute(commandsToExecute);
            }
        }
        catch (Exception e)
        {
            ExecuteHelp(e);
        }
    }

    private void ProcessSubcommand(string arg, List<IClyshCommand> commandsToExecute)
    {
        CheckLastOptionStatus();
        lastOption = null;
        lastCommand.Executed = true;
        lastCommand = GetCommandFromArg(lastCommand, arg);
        commandsToExecute.Add(lastCommand);
    }

    private void ProcessOption(string arg)
    {
        CheckLastOptionStatus();

        var key = IsOptionFull(arg) ? arg[2..] : arg[1..];

        if (!lastCommand.HasOption(key))
            throw new InvalidOperationException($"The option '{arg}' is invalid.");

        lastOption = lastCommand.GetOption(key);

        HandleOptionGroup();

        lastOption.Selected = true;
    }

    private bool IsSubcommand(string arg)
    {
        return lastCommand.HasSubcommand(arg);
    }

    private void HandleOptionGroup()
    {
        if (lastOption?.Group != null)
        {
            var oldOptionOfGroupSelected = lastCommand
                .GetOptionFromGroup(lastOption.Group.Id);

            if (oldOptionOfGroupSelected != null)
                oldOptionOfGroupSelected.Selected = false;
        }
    }

    private bool OptionHelp()
    {
        return lastOption is { Id: "help" };
    }

    private void CheckLastCommandStatus()
    {
        if (lastCommand.RequireSubcommand && !lastCommand.HasAnySubcommandExecuted())
            throw new InvalidOperationException($"You need to provide some subcommand to command '{lastCommand.Id}'");
    }

    private void ProcessParameter(string arg)
    {
        if (arg.IsEmpty()) return;
        
        if (ArgIsParameterById(arg))
            ProcessParameterById(arg);
        else
            ProcessParameterByPosition(arg);
    }

    private void ProcessParameterByPosition(string arg)
    {
        if (lastOption == null)
            throw new InvalidOperationException("You can't put parameters without any option that accept it.");
        
        if (!lastOption.Parameters.WaitingForAny())
            throw new InvalidOperationException(
                $"The parameter data '{arg}' is out of bound for option: {lastOption.Id}.");

        lastOption.Parameters.Last().Data = arg;
    }

    private void ProcessParameterById(string arg)
    {
        if (lastOption == null)
            throw new InvalidOperationException("You can't put parameters without any option that accept it.");

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

    private void CheckLastOptionStatus()
    {
        if (lastOption != null)
        {
            if (lastOption.Parameters.WaitingForRequired())
                throw new InvalidOperationException(
                    $"Required parameters [{lastOption.Parameters.RequiredToString()}] is missing for option: {lastOption.Id} (shortcut: {lastOption.Shortcut ?? "<null>"})");
        }
    }

    private static bool ArgIsParameterById(string arg)
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

        Completed = true;
    }

    private static IClyshCommand GetCommandFromArg(IClyshCommand lastCommand, string arg)
    {
        var order = lastCommand.Order + 1;
        lastCommand = lastCommand.SubCommands[arg];
        lastCommand.Order = order;
        return lastCommand;
    }

    private void ExecuteHelp(Exception? exception = null)
    {
        if (exception == null)
            View.PrintHelp(lastCommand);
        else
            View.PrintHelp(lastCommand, exception);

        Completed = true;
    }

    private static bool IsOption(string arg)
    {
        return arg.StartsWith("-");
    }

    private static bool IsOptionFull(string arg)
    {
        return arg.StartsWith("--");
    }
}