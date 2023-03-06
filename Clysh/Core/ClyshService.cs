using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// The main service of <see cref="Clysh"/>
/// </summary>
public sealed class ClyshService : IClyshService
{
    private readonly bool _disableAudit;
    
    private readonly Dictionary<string, string> _messages = new();
    
    private readonly Dictionary<string, ClyshOption> _optionsFromGroup = new();
    
    private readonly List<ClyshAudit> _audits = new();

    private readonly List<ClyshCommand> _commandsToExecute = new();

    private ClyshCommand _lastCommand;

    private ClyshOption? _lastOption;

    /// <summary>
    /// The constructor of service
    /// </summary>
    /// <param name="setup">The setup of <see cref="Clysh"/></param>
    /// <param name="disableAudit">Indicates if the service shouldn't validate production rules</param>
    [ExcludeFromCodeCoverage]
    public ClyshService(IClyshSetup setup, bool disableAudit = false) : this(setup, new ClyshConsole(),
        disableAudit)
    {
    }

    [ExcludeFromCodeCoverage]
    private ClyshService(IClyshSetup setup, IClyshConsole clyshConsole, bool disableAudit): this(setup.RootCommand, new ClyshView(clyshConsole, setup.Data), disableAudit, setup.Data.Messages)
    {
    }

    /// <summary>
    /// The constructor of service
    /// </summary>
    /// <param name="rootCommand">The root command to be executed</param>
    /// <param name="view">The view to output</param>
    /// <param name="disableAudit">Indicates if the service shouldn't validate production rules</param>
    /// <param name="customMessages">Custom messages to show</param>
    public ClyshService(ClyshCommand rootCommand, IClyshView view, bool disableAudit = false, Dictionary<string, string>? customMessages = null)
    {
        RootCommand = rootCommand;
        RootCommand.Order = 0;
        _lastCommand = RootCommand;
        View = view;
        _disableAudit = disableAudit;

        LoadMessages(customMessages);
    }

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
            InitProcess();

            ProcessArgs(args);

            FinishProcess();
        }
        catch (Exception e)
        {
            ExecuteHelp(e);
        }
    }

    private void LoadCustomMessages(Dictionary<string, string> setupMessages)
    {
        foreach (var setupMessage in setupMessages
                     .Where(setupMessage => _messages.ContainsKey(setupMessage.Key)))
        {
            _messages[setupMessage.Key] = setupMessage.Value;
        }
    }

    private void LoadMessages(Dictionary<string, string>? customMessages)
    {
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
    }

    private void InitProcess()
    {
        _commandsToExecute.Add(_lastCommand);
        
        if (!_disableAudit)
            Audit();
    }

    private void ProcessArgs(IEnumerable<string> args)
    {
        foreach (var arg in args)
        {
            if (!IsOption(arg))
                ProcessAnotherArgumentType(arg);
            else
            {
                ProcessOption(arg);

                if (OptionHelp() || OptionVersion())
                    break;
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
            ProcessSubcommand(arg);
        else //is parameter
            ProcessParameter(arg);
    }

    private void FinishProcess()
    {
        if (OptionHelp())
            ExecuteHelp();
        else if (OptionVersion())
            ExecuteVersion();
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
            View.Print($"({i}) >> {audit}\n");
        }

        View.PrintSeparator("AUDIT END");
        View.PrintEmpty();
    }

    private void Audit()
    {
        Audit(RootCommand);

        var hasAnyError = _audits.Any(x => x.AnyError());
        
        if (hasAnyError)
            LogAuditMessages();
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

    private static void AuditCommand(ClyshCommand cmd, ClyshAudit audit)
    {
        if (cmd.Action == null)
            audit.Messages.Add(string.Format(ClyshMessages.ErrorOnValidateCommandAction, cmd.Id));
    }

    private void ProcessSubcommand(string arg)
    {
        CheckLastOptionStatus();
        SetCommandToExecute(arg);
    }

    private void ProcessOption(string arg)
    {
        CheckLastOptionStatus();

        var key = IsOptionFull(arg) ? arg[2..] : arg[1..];

        if (!_lastCommand.HasOption(key))
            ShowErrorMessage("InvalidOption", arg);

        _lastOption = _lastCommand.GetOption(key);

        HandleOptionGroup();
        
        if (_lastOption.Group == null)
            _lastOption.Selected = true;
        
        if (OptionDebug())
            View.Debug = true;
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
        var waitingForAnySubcommand = _lastCommand.RequireSubcommand && !_lastCommand.AnySubcommandInputed();
        
        if (waitingForAnySubcommand)
            ShowErrorMessage("InvalidSubcommand", _lastCommand.Id);
    }

    private void ShowErrorMessage(string messageId, params object[] parameters)
    {
        throw new ValidationException(string.Format(_messages[messageId], parameters));
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
        if (_lastOption == null)
            ShowErrorMessage("InvalidArgument", arg);

        if (!_lastOption!.Parameters.WaitingForAny())
            ShowErrorMessage("InvalidParameter", arg, _lastOption.Id);

        var lastParameter = _lastOption.Parameters.Last();
        
        lastParameter.Data = arg;
        lastParameter.Filled = true;
    }

    private void ProcessParameterById(string arg)
    {
        if (_lastOption == null)
            ShowErrorMessage("InvalidArgument", arg);
        
        var parameter = arg.Split(":");

        var id = parameter[0];
        var data = parameter[1];

        if (_lastOption!.Parameters.Has(id))
        {
            if (!_lastOption.Parameters[id].Data.IsEmpty())
                ShowErrorMessage("ParameterConflict", id, _lastOption.Id);

            _lastOption.Parameters[id].Data = data;
            _lastOption.Parameters[id].Filled = true;
        }
        else
            ShowErrorMessage("IncorrectParameter", id, _lastOption.Id);
    }

    private void CheckLastOptionStatus()
    {
        var waitingForRequiredParameters = _lastOption != null && _lastOption.Parameters.WaitingForRequired();
        
        if (waitingForRequiredParameters)
            ShowErrorMessage("RequiredParameters", _lastOption!.Parameters.RequiredToString(), _lastOption.Id, _lastOption.Shortcut ?? "<no_shortcut>");
    }

    private static bool ArgIsParameterById(string arg)
    {
        return arg.Contains(':');
    }

    private void ExecuteCommands()
    {
        foreach (var command in _commandsToExecute.OrderBy(x => x.Order))
        {
            if (command.Action != null)
                command.Action(command, View);
            else if (!command.RequireSubcommand)
                throw new ClyshException($"Action null (NOT READY TO PRODUCTION). Command: {command.Id}");
        }
    }

    private void SetCommandToExecute(string arg)
    {
        _lastOption = null;
        _lastCommand.Inputed = true;
        
        var actualCommand = _lastCommand.SubCommands[GetCommandId(arg)];

        if (actualCommand.IgnoreParents)
        {
            actualCommand.Order = 0;
            _commandsToExecute.ForEach(c => c.Order=-1);
            _commandsToExecute.Clear();
        }
        else
        {
            actualCommand.Order = _lastCommand.Order + 1;
        }
        
        _commandsToExecute.Add(actualCommand);
        _lastCommand = actualCommand;
    }

    private void ExecuteHelp(Exception? exception = null)
    {
        if (exception == null)
            View.PrintHelp(_lastCommand);
        else
            View.PrintException(exception);
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