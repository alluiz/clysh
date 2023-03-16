using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Clysh.Helper;
using Microsoft.Extensions.Logging;

namespace Clysh.Core;

/// <summary>
/// The main service of <see cref="Clysh"/>
/// </summary>
public sealed class ClyshService : IClyshService
{
    /// <summary>
    /// The constructor of service
    /// </summary>
    /// <param name="setup">The setup of <see cref="Clysh"/></param>
    /// <param name="logger">The logger of the system</param>
    /// <param name="disableAudit">Indicates if the service shouldn't validate production rules</param>
    [ExcludeFromCodeCoverage]
    public ClyshService(IClyshSetup setup, bool disableAudit = false, ILogger<ClyshService>? logger = null) : this(setup, new ClyshConsole(),
        disableAudit, logger)
    {
    }

    /// <summary>
    /// The constructor of service
    /// </summary>
    /// <param name="rootCommand">The root command to be executed</param>
    /// <param name="view">The view to output</param>
    /// <param name="logger">The logger of the system</param>
    /// <param name="disableAudit">Indicates if the service shouldn't validate production rules</param>
    /// <param name="customMessages">Custom messages to show</param>
    public ClyshService(ClyshCommand rootCommand, IClyshView view, bool disableAudit = false, Dictionary<string, string>? customMessages = null, ILogger<ClyshService>? logger = null)
    {
        RootCommand = rootCommand;
        RootCommand.Order = 0;
        _lastCommand = RootCommand;
        View = view;
        _logger = logger;
        _disableAudit = disableAudit;

        LoadMessages(customMessages);
    }

    [ExcludeFromCodeCoverage]
    private ClyshService(IClyshSetup setup, IClyshConsole clyshConsole, bool disableAudit, ILogger<ClyshService>? logger): this(setup.RootCommand, new ClyshView(clyshConsole, setup.Data), disableAudit, setup.Data.Messages, logger)
    {
    }
    
    private readonly bool _disableAudit;
    
    private readonly Dictionary<string, string> _messages = new();
    
    private readonly Dictionary<string, ClyshOption> _optionsFromGroup = new();
    
    private readonly ILogger<ClyshService>? _logger;
    
    private readonly List<ClyshAudit> _audits = new();

    private readonly List<ClyshCommand> _commandsToExecute = new();

    private ClyshCommand _lastCommand;

    private ClyshOption? _lastOption;

    /// <summary>
    /// The root command
    /// </summary>
    public ClyshCommand RootCommand { get; }

    /// <summary>
    /// The view
    /// </summary>
    public IClyshView View { get; }

    /// <summary>
    /// Execute the CLI with program arguments
    /// </summary>
    /// <param name="args">The arguments of CLI</param>
    public void Execute(IEnumerable<string> args)
    {
        try
        {
            _logger?.LogInformation("Executing CLI service...");
            
            InitProcess();

            ProcessArgs(args);

            FinishProcess();
        }
        catch (Exception e)
        {
            ExecuteHelp(e);
        }
        
        _logger?.LogInformation("CLI service was executed.");
    }

    private void LoadCustomMessages(Dictionary<string, string> setupMessages)
    {
        foreach (var setupMessage in setupMessages
                     .Where(setupMessage => _messages.ContainsKey(setupMessage.Key)))
        {
            _messages[setupMessage.Key] = setupMessage.Value;
            _logger?.LogDebug("Message '{key}' was replaced by custom message.", setupMessage.Key);
        }
        
        _logger?.LogInformation("Custom messages was loaded.");
    }

    private void LoadMessages(Dictionary<string, string>? customMessages)
    {
        _logger?.LogInformation("Loading messages to show...");
        
        LoadDefaultMessages();

        if (customMessages != null)
            LoadCustomMessages(customMessages);
    }

    private void LoadDefaultMessages()
    {
        _messages.Add("InvalidOption", ClyshMessages.ErrorOnValidateUserInputOption);
        _messages.Add("InvalidSubcommand", ClyshMessages.ErrorOnValidateUserInputSubcommand);
        _messages.Add("InvalidArgument", ClyshMessages.ErrorOnValidateUserInputArgument);
        _messages.Add("InvalidParameter", ClyshMessages.ErrorOnValidateUserInputArgumentOutOfBound);
        _messages.Add("IncorrectParameter", ClyshMessages.ErrorOnValidateUserInputParameterInvalid);
        _messages.Add("ParameterConflict", ClyshMessages.ErrorOnValidateUserInputParameterConflict);
        _messages.Add("RequiredParameters", ClyshMessages.ErrorOnValidateUserInputRequiredParameters);
        
        _logger?.LogInformation("Default messages was loaded.");
    }

    private void InitProcess()
    {
        _logger?.LogInformation("Initializing process...");
        
        _logger?.LogDebug("Setting root as the first command to execute.");
        _commandsToExecute.Add(_lastCommand);

        if (!_disableAudit)
        {
            Audit();
        }
        else
            _logger?.LogInformation("Audit is DISABLED.");
    }

    private void ProcessArgs(IEnumerable<string> args)
    {
        _logger?.LogInformation("Processing input args...");
        
        foreach (var arg in args)
        {
            _logger?.LogInformation("Processing arg '{arg}'...", arg);
            
            if (!IsOption(arg))
            {
                _logger?.LogDebug("Arg '{arg}' is NOT an OPTION.", arg);
                ProcessAnotherArgumentType(arg);
            }
            else
            {
                _logger?.LogDebug("Arg '{arg}' is an OPTION.", arg);
                ProcessOption(arg);

                if (OptionHelp() || OptionVersion())
                {
                    _logger?.LogWarning("VERSION or HELP is executed by user. PROCESS SKIPPED.");
                    break;
                }
            }
        }
    }

    private bool OptionVersion()
    {
        return _lastOption is { Id: "version" };
    }

    private void ProcessAnotherArgumentType(string arg)
    {
        if (IsSubcommand(arg))
        {
            _logger?.LogDebug("Arg '{arg}' is a SUBCOMMAND.", arg);
            ProcessSubcommand(arg);
        }
        else //is parameter
        {
            _logger?.LogDebug("Arg '{arg}' is a PARAMETER.", arg);
            ProcessArgument(arg);
        }
    }

    private void FinishProcess()
    {
        if (OptionHelp())
        {
            _logger?.LogInformation("Executing HELP...");
            ExecuteHelp();
            _logger?.LogInformation("HELP execution was done.");
        }
        else if (OptionVersion())
        {
            _logger?.LogInformation("Executing VERSION...");
            ExecuteVersion();
            _logger?.LogInformation("VERSION execution was done.");
        }
        else
        {
            _lastCommand.Inputed = true;
            CheckGroups();
            CheckLastCommandStatus();
            CheckLastOptionStatus();
            ExecuteCommands();
        }
    }

    private void CheckGroups()
    {
        foreach (var option in _optionsFromGroup.Values) 
            option.Selected = true;
    }

    private void ExecuteVersion()
    {
        View.PrintVersion();
    }

    private bool OptionDebug()
    {
        return _lastOption is { Id: "debug" };
    }

    private void LogAuditMessages()
    {
        View.PrintSeparator("AUDITING");
        View.PrintEmpty();
        View.Print("YOUR CLI ARE NOT READY TO PRODUCTION");
        View.Print("You can disable this alert with constructor param 'disableAudit' = true");
        View.PrintEmpty();
        View.PrintSeparator("LIST TO CHECK");
        View.PrintEmpty();

        var listOfAuditsWithAnyError = _audits.Where(x => x.AnyError()).ToArray();
        
        for (var i = 0; i < listOfAuditsWithAnyError.Length; i++)
        {
            var audit = listOfAuditsWithAnyError[i];
            _logger?.LogDebug("Printing audit error '{}' to user...", audit);
            View.Print($"({i}) >> {audit}\n");
        }

        View.PrintSeparator("AUDIT END");
        View.PrintEmpty();
    }

    private void Audit()
    {
        _logger?.LogInformation("Auditing service...");

        Audit(RootCommand);

        var hasAnyError = _audits.Any(x => x.AnyError());

        if (!hasAnyError) return;
        
        _logger?.LogWarning("Some audit errors was found.");
        
        LogAuditMessages();

        _logger?.LogInformation("Auditing service terminated.");
    }

    private void Audit(ClyshCommand cmd)
    {
        var audit = new ClyshAudit(cmd);

        _audits.Add(audit);

        AuditCommand(cmd, audit);

        var hasSubcommand = cmd.SubCommands.Any();

        if (!hasSubcommand) return;
        
        foreach (var subCommand in cmd.SubCommands.Values)
            Audit(subCommand);
    }

    private void AuditCommand(ClyshCommand cmd, ClyshAudit audit)
    {
        _logger?.LogInformation("Auditing command '{commandId}'...", cmd.Id);

        if (cmd.Action != null || cmd.Abstract) return;
        
        audit.Messages.Add(string.Format(ClyshMessages.ErrorOnValidateCommandAction, cmd.Id));
        _logger?.LogDebug("Command action is NULL.");
        _logger?.LogDebug("Command is NOT abstract.");
        _logger?.LogWarning("Command '{commandId}' is NOT ready to production.", cmd.Id);
    }

    private void ProcessSubcommand(string arg)
    {
        _logger?.LogInformation("Processing subcommand '{commandId}'...", arg);
        CheckLastOptionStatus();
        SetCommandToExecute(arg);
        _logger?.LogInformation("Subcommand '{commandId}' was processed.", arg);
    }

    private void ProcessOption(string arg)
    {
        _logger?.LogInformation("Processing option '{arg}'...", arg);
        
        CheckLastOptionStatus();

        var key = IsOptionFull(arg) ? arg[2..] : arg[1..];

        if (!_lastCommand.HasOption(key))
        {
            _logger?.LogError("OPTION '{optionId}' was not found by last command '{commandId}'.", key, _lastCommand.Id);
            ShowErrorMessage("InvalidOption", arg);
        }

        _lastOption = _lastCommand.GetOption(key);

        HandleOptionGroup();

        if (_lastOption.Group == null)
        {
            _lastOption.Selected = true;
            _logger?.LogInformation("Option '{arg}' was selected.", arg);
        }
        else
        {
            _logger?.LogDebug("Option '{arg}' group is '{group}'.", arg, _lastOption.Group);
        }

        if (OptionDebug())
        {
            _logger?.LogInformation("DEBUG mode is enabled by user.");
            View.Debug = true;
        }

        _logger?.LogInformation("Option was processed.");
        
    }

    private bool IsSubcommand(string arg)
    {
        var commandId = GetCommandId(arg);
        return _lastCommand.HasSubcommand(commandId);
    }

    private string GetCommandId(string arg)
    {
        return $"{_lastCommand.Id}.{arg}";
    }

    private void HandleOptionGroup()
    {
        if (_lastOption?.Group == null) return;

        _optionsFromGroup[_lastOption.Group.Id] = _lastOption;
    }

    private bool OptionHelp()
    {
        return _lastOption is { Id: "help" };
    }

    private void CheckLastCommandStatus()
    {
        var waitingForAnySubcommand = _lastCommand.Abstract && !_lastCommand.AnySubcommandInputed();
        
        if (waitingForAnySubcommand)
        {
            _logger?.LogError("The last command '{lastCommandId}' was waiting for a COMMAND but NOTHING was provided.", _lastCommand.Id);
            ShowErrorMessage("InvalidSubcommand", _lastCommand.Id);
        }
    }

    private void ShowErrorMessage(string messageId, params object[] parameters)
    {
        throw new ValidationException(string.Format(_messages[messageId], parameters));
    }

    private void ProcessArgument(string arg)
    {
        _logger?.LogInformation("Processing argument value '{arg}'...", arg);
        if (arg.IsEmpty())
        {
            _logger?.LogDebug("Argument '{arg}' is EMPTY.", arg);
            return;
        }

        if (ArgIsParameterById(arg))
        {
            _logger?.LogDebug("Parameter '{arg}' is by ID.", arg);
            ProcessParameterById(arg);
        }
        else
        {
            _logger?.LogDebug("Argument '{arg}' is by POSITION.", arg);
            ProcessParameterByPosition(arg);
        }
    }

    private void ProcessParameterByPosition(string arg)
    {
        _logger?.LogInformation("Processing parameter value '{arg}' by position...", arg);
        
        if (_lastOption == null)
        {
            _logger?.LogError("Unexpected PARAMETER type because last option is NULL.");
            ShowErrorMessage("InvalidArgument", arg);
        }

        if (!_lastOption!.Parameters.WaitingForAny())
        {
            _logger?.LogError("Unexpected PARAMETER type because last option is NOT waiting for a parameter.");
            ShowErrorMessage("InvalidParameter", arg, _lastOption.Id);
        }

        var lastParameter = _lastOption.Parameters.Last();
        
        lastParameter.Data = arg;
        lastParameter.Filled = true;
        
        _logger?.LogInformation("Parameter '{parameterId}' was filled by '{arg}'.", lastParameter.Id, arg);
    }

    private void ProcessParameterById(string arg)
    {
        _logger?.LogInformation("Processing parameter '{arg}' by id...", arg);

        if (_lastOption == null)
        {
            _logger?.LogError("Unexpected PARAMETER type because last option is NULL.");
            ShowErrorMessage("InvalidArgument", arg);
        }

        var parameter = arg.Split(":=");

        var id = parameter[0];
        _logger?.LogDebug("Parameter ID: '{id}'.", id);
        
        var data = parameter[1];
        _logger?.LogDebug("Parameter DATA: '{data}'.", data);

        if (_lastOption!.Parameters.Has(id))
        {
            var parameterData = _lastOption.Parameters[id].Data;
            
            if (!parameterData.IsEmpty())
            {
                _logger?.LogError("PARAMETER was already filled by DATA '{parameterData}'.", parameterData);
                ShowErrorMessage("ParameterConflict", id, _lastOption.Id);
            }

            _lastOption.Parameters[id].Data = data;
            _lastOption.Parameters[id].Filled = true;
        }
        else
        {
            _logger?.LogError("PARAMETER_ID was not found by last OPTION '{lastOptionId}'.", _lastOption.ToString());
            ShowErrorMessage("IncorrectParameter", id, _lastOption.Id);
        }
        
        _logger?.LogInformation("Parameter '{arg}' was filled.", arg);
    }

    private void CheckLastOptionStatus()
    {
        var waitingForRequiredParameters = _lastOption != null && _lastOption.Parameters.WaitingForRequired();

        if (waitingForRequiredParameters)
        {
            _logger?.LogError("The last command '{lastCommandId}' option '{optionId}' was waiting for a PARAMETER but a COMMAND was provided.", _lastCommand.Id, _lastOption!.Id);
            ShowErrorMessage("RequiredParameters", _lastOption!.Parameters.RequiredToString(), _lastOption.Id, _lastOption.Shortcut ?? "<no_shortcut>");
        }
    }

    private static bool ArgIsParameterById(string arg)
    {
        return arg.Contains(":=");
    }

    private void ExecuteCommands()
    {
        foreach (var command in _commandsToExecute.OrderBy(x => x.Order))
        {
            _logger?.LogInformation("Processing command '{commandId}'...", command.Id);
            
            if (command.Action != null)
            {
                _logger?.LogInformation("Executing action of '{commandId}'...", command.Id);
                command.Action(command, View);
                _logger?.LogInformation("Command '{commandId}' action was executed.", command.Id);
            } 
            else if (command.ActionV2 != null)
            {
                _logger?.LogInformation("Executing action V2 of '{commandId}'...", command.Id);
                command.ActionV2(new Cly(command, View));
                _logger?.LogInformation("Command '{commandId}' action V2 was executed.", command.Id);
            }
            else if (!command.Abstract)
            {
                _logger?.LogError("Command '{commandId}' action is NULL and command is not ABSTRACT.", command.Id);
                throw new ClyshException($"Action is null (NOT READY TO PRODUCTION). Command: {command.Id}");
            }

            _logger?.LogInformation("Command '{commandId}' was processed.", command.Id);
        }
    }

    private void SetCommandToExecute(string arg)
    {
        _lastOption = null;
        _logger?.LogDebug("Set last OPTION to NULL.");
        
        _lastCommand.Inputed = true;
        _logger?.LogDebug("Set last COMMAND to INPUTED.");

        _logger?.LogDebug("Getting actual COMMAND from arg.");
        var actualCommand = _lastCommand.SubCommands[GetCommandId(arg)];

        if (actualCommand.IgnoreParents)
        {
            _logger?.LogDebug("Command '{commandId}' IGNORE PARENTS action is TRUE.", actualCommand.Id);
            _logger?.LogInformation("Command '{commandId}' parents will not be executed.", actualCommand.Id);
            actualCommand.Order = 0;
            _commandsToExecute.ForEach(c => c.Order=-1);
            _commandsToExecute.Clear();
            _logger?.LogInformation("Commands execution list was reset.");
        }
        else
        {
            actualCommand.Order = _lastCommand.Order + 1;
        }
        
        _logger?.LogDebug("Command '{commandId}' order is {order}.", actualCommand.Id, actualCommand.Order);
        
        _commandsToExecute.Add(actualCommand);
        _logger?.LogDebug("Command '{commandId}' was added to execution list.", actualCommand.Id);
        
        _lastCommand = actualCommand;
        _logger?.LogDebug("Set last COMMAND to '{commandId}'.", actualCommand.Id);
    }

    private void ExecuteHelp(Exception? exception = null)
    {
        if (exception == null)
            View.PrintHelp(_lastCommand);
        else
        {
            _logger?.LogWarning("The user will receive an error.");
            View.PrintException(exception);
            _logger?.LogWarning("The user received an error.");
        }
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