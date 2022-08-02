using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// The main service of <see cref="Clysh"/>
/// </summary>
public class ClyshService : IClyshService
{
    private const string YourCommandDoesNotHaveAnActionConfigured =
        "Your command does NOT have an action configured. Command: '{0}'.";

    private const string YourCommandDoesNotHaveASubcommandConfigured =
        "Your command does NOT have a subcommand configured. Command: '{0}'.";

    /// <summary>
    /// The root command
    /// </summary>
    public IClyshCommand RootCommand { get; }

    /// <summary>
    /// The view
    /// </summary>
    public IClyshView View { get; set; }

    private ClyshOption? lastOption;

    private IClyshCommand lastCommand;

    private readonly bool disableAudit;

    private readonly List<ClyshAudit> audits;

    private List<IClyshCommand> commandsToExecute;

    /// <summary>
    /// The constructor of service
    /// </summary>
    /// <param name="setup">The setup of <see cref="Clysh"/></param>
    /// <param name="disableAudit">Indicates if the service shouldn't validate production rules</param>
    [ExcludeFromCodeCoverage]
    public ClyshService(ClyshSetup setup, bool disableAudit = false) : this(setup, new ClyshConsole(),
        disableAudit)
    {
    }

    [ExcludeFromCodeCoverage]
    private ClyshService(ClyshSetup setup, IClyshConsole clyshConsole, bool disableAudit = false)
    {
        this.disableAudit = disableAudit;
        RootCommand = setup.RootCommand;
        RootCommand.Order = 0;
        lastCommand = RootCommand;
        audits = new List<ClyshAudit>();
        commandsToExecute = new List<IClyshCommand>();
        View = new ClyshView(clyshConsole, setup.Data);
    }

    /// <summary>
    /// The constructor of service
    /// </summary>
    /// <param name="rootCommand">The root command to be executed</param>
    /// <param name="view">The view to output</param>
    /// <param name="disableAudit">Indicates if the service shouldn't validate production rules</param>
    public ClyshService(IClyshCommand rootCommand, IClyshView view, bool disableAudit = false)
    {
        this.disableAudit = disableAudit;
        RootCommand = rootCommand;
        RootCommand.Order = 0;
        lastCommand = RootCommand;
        View = view;
        audits = new List<ClyshAudit>();
        commandsToExecute = new List<IClyshCommand>();
    }

    /// <summary>
    /// Execute the CLI with program arguments
    /// </summary>
    /// <param name="args">The arguments of CLI</param>
    public void Execute(string[] args)
    {
        try
        {
            InitProcess();

            ProcessArgs(args);

            FinishProcess();
        }
        catch (Exception e)
        {
            ExecuteHelp(e);
        }
    }

    private void InitProcess()
    {
        commandsToExecute = new List<IClyshCommand> { lastCommand };
        View.Debug = false;
        
        AuditClysh();
    }

    private void ProcessArgs(IEnumerable<string> args)
    {
        foreach (var arg in args)
        {
            if (IsOption(arg))
            {
                ProcessOption(arg);

                if (OptionHelp())
                    break;
            }
            else
            {
                if (IsSubcommand(arg))
                    ProcessSubcommand(arg);
                else //is parameter
                    ProcessParameter(arg);
            }
        }
    }

    private void FinishProcess()
    {
        if (OptionHelp())
        {
            ExecuteHelp();
        }
        else
        {
            lastCommand.Executed = true;
            CheckLastCommandStatus();
            CheckLastOptionStatus();
            ExecuteCommands();
        }
    }

    private bool OptionDebug()
    {
        return lastOption is { Id: "debug" };
    }

    private void AuditLogMessages()
    {
        View.PrintSeparator("AUDITING");
        View.PrintEmpty();
        View.Print("YOUR CLI ARE NOT READY TO PRODUCTION. ");
        View.Print("You can disable this alert with constructor param 'disableAudit' = true");
        View.PrintEmpty();
        View.PrintSeparator("LIST TO CHECK");
        View.PrintEmpty();

        var array = audits.Where(x => x.AnyError()).ToArray();
        
        for (var i = 0; i < array.Length; i++)
        {
            var audit = array[i];
            View.Print($"({i}) >> {audit}\n");
        }

        View.PrintSeparator("AUDIT END");
        View.PrintEmpty();
    }

    private void AuditClysh()
    {
        if (disableAudit) return;

        AuditRecursive(RootCommand);

        if (audits.Any(x => x.AnyError()))
            AuditLogMessages();
    }

    private void AuditRecursive(IClyshCommand cmd)
    {
        var audit = new ClyshAudit(cmd);

        audits.Add(audit);

        AuditCommand(cmd, audit);

        if (cmd.SubCommands.Any())
        {
            foreach (var subCommand in cmd.SubCommands.Values)
                AuditRecursive(subCommand);
        }
    }

    private static void AuditCommand(IClyshCommand cmd, ClyshAudit audit)
    {
        if (cmd.RequireSubcommand)
        {
            if (!cmd.SubCommands.Any())
                audit.Messages.Add(string.Format(YourCommandDoesNotHaveASubcommandConfigured, cmd.Id));
        }
        else
        {
            if (cmd.Action == null)
                audit.Messages.Add(string.Format(YourCommandDoesNotHaveAnActionConfigured, cmd.Id));
        }
    }

    private void ProcessSubcommand(string arg)
    {
        CheckLastOptionStatus();
        lastOption = null;
        lastCommand.Executed = true;
        SetCommandToExecute(arg);
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
        
        if (OptionDebug())
            View.Debug = true;
    }

    private bool IsSubcommand(string arg)
    {
        var commandId = GetCommandId(arg);
        return lastCommand.HasSubcommand(commandId);
    }

    private string GetCommandId(string arg)
    {
        return $"{lastCommand.Id}.{arg}";
    }

    private void HandleOptionGroup()
    {
        if (lastOption?.Group != null)
        {
            var oldOptionOfGroupSelected = lastCommand
                .GetOptionFromGroup(lastOption.Group);

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
        var waitingForAnySubcommand = lastCommand.RequireSubcommand && !lastCommand.HasAnySubcommandExecuted();
        
        if (waitingForAnySubcommand)
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
        lastOption.Parameters.Last().Filled = true;
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
            if (!lastOption.Parameters[id].Data.IsEmpty())
                throw new InvalidOperationException(
                    $"The parameter '{id}' is already filled for option: {lastOption.Id}.");

            lastOption.Parameters[id].Data = data;
            lastOption.Parameters[id].Filled = true;
        }
        else
            throw new InvalidOperationException(
                $"The parameter '{id}' is invalid for option: {lastOption.Id}.");
    }

    private void CheckLastOptionStatus()
    {
        var waitingForRequiredParameters = lastOption != null && lastOption.Parameters.WaitingForRequired();
        
        if (waitingForRequiredParameters)
            throw new InvalidOperationException(
                $"Required parameters [{lastOption!.Parameters.RequiredToString()}] is missing for option: {lastOption.Id} (shortcut: {lastOption.Shortcut ?? "<null>"})");
    }

    private static bool ArgIsParameterById(string arg)
    {
        return arg.Contains(':');
    }

    private void ExecuteCommands()
    {
        foreach (var command in commandsToExecute.OrderBy(x => x.Order))
        {
            if (command.Action != null)
                command.Action(command, command.Options, View);
            else if (!command.RequireSubcommand)
                throw new ClyshException($"Action null (NOT READY TO PRODUCTION). Command: {command.Id}");
        }
    }

    private void SetCommandToExecute(string arg)
    {
        var order = lastCommand.Order + 1;
        lastCommand = lastCommand.SubCommands[GetCommandId(arg)];
        lastCommand.Order = order;
        commandsToExecute.Add(lastCommand);
    }

    private void ExecuteHelp(Exception? exception = null)
    {
        if (exception == null)
            View.PrintHelp(lastCommand);
        else
            View.PrintHelp(lastCommand, exception);
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