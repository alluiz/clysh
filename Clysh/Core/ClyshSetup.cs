using System.IO.Abstractions;
using System.Text.RegularExpressions;
using Clysh.Core.Builder;
using Clysh.Data;
using Clysh.Helper;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using Microsoft.Extensions.Logging;

namespace Clysh.Core;

/// <summary>
/// The setup of <see cref="Clysh"/>.
/// Use it to process data from a declarative file.
/// </summary>
public class ClyshSetup : IClyshSetup
{
    /// <summary>
    /// The <see cref="ClyshSetup"/> constructor.
    /// </summary>
    /// <param name="path">The path of a declarative file in YAML or JSON format. For example: cli.yml</param>
    /// <param name="fs">The file system object. Could be used for mocking declarative file.</param>
    /// <param name="logger">The logger of the system If it is null, the log will not be printed.</param>
    public ClyshSetup(string path, IFileSystem fs, ILogger<ClyshSetup>? logger = null)  
    {
        _logger = logger;
        _fs = fs;
        _path = path;
        Load();
    }

    /// <summary>
    /// The <see cref="ClyshSetup"/> constructor with default FileSystem.
    /// </summary>
    /// <param name="path">The path of a declarative file in YAML or JSON format. For example: cli.yml</param>
    /// <param name="logger">The logger of the system If it is null, the log will not be printed.</param>
    public ClyshSetup(string path, ILogger<ClyshSetup>? logger = null) : this(path, new FileSystem(), logger)
    {
    }

    /// <summary>
    /// The CLI Root command
    /// </summary>
    public ClyshCommand RootCommand { get; private set; } = default!;

    /// <summary>
    /// The CLI Data
    /// </summary>
    public ClyshData Data { get; private set; } = new();

    /// <summary>
    /// The CLI Commands
    /// </summary>
    public ClyshMap<ClyshCommand> Commands { get; } = new();

    private readonly ClyshMap<ClyshGroup> _groups = new();
    private readonly Dictionary<string, List<OptionData>> _commandGlobalOptions = new();
    private readonly IFileSystem _fs;
    private readonly ILogger<ClyshSetup>? _logger;
    private readonly string _path;

    /// <summary>
    /// Bind your command action
    /// </summary>
    /// <param name="commandId">The command id</param>
    /// <param name="action">The action to be executed</param>
    public void BindAction(string commandId, Action<IClyshCommand, IClyshView> action)
    {
        if (!Commands.Has(commandId))
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnSetupBindAction, commandId));

        var command = Commands[commandId];
        command.Action = action;
    }
    
    /// <summary>
    /// Bind your command action
    /// </summary>
    /// <param name="commandId">The command id</param>
    /// <param name="action">The action that implements the IClyshAction interface</param>
    public void BindAction<T>(string commandId, T action) where T: IClyshAction
    {
        BindAction(commandId, action.Execute);
    }

    private void ExtractDataFromFileSystem()
    {
        _logger?.LogInformation("Extracting data from file '{path}'...", _path);

        CheckFileExists();
        GetData();

        _logger?.LogInformation("Data extracted.");
    }

    private void GetData()
    {
        var extension = _fs.Path.GetExtension(_path);

        _logger?.LogDebug("File extension is '{extension}'", extension);

        Data = extension switch
        {
            ".json" => Deserialize(DeserializerType.Json),
            ".yml" => Deserialize(DeserializerType.Yaml),
            ".yaml" => Deserialize(DeserializerType.Yaml),
            _ => throw new ClyshException(string.Format(ClyshMessages.ErrorOnSetupLoadFileExtension, _path))
        };
    }

    private void CheckFileExists()
    {
        var fileExists = _fs.File.Exists(_path);

        _logger?.LogDebug("File exists: {fileExists}", fileExists);

        if (!fileExists)
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnSetupLoadFilePath, _path));
    }

    private void CreateCommandsFromData()
    {
        try
        {
            _logger?.LogInformation("Getting commands data...");
            VerifyCommands();
            CreateCommands();
        }
        catch (ClyshException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ClyshException(ClyshMessages.ErrorOnSetupCommands, e);
        }
    }

    private void CreateCommands()
    {
        _logger?.LogInformation("Creating commands...");
        
        var rootData = GetRootData();

        _logger?.LogInformation("Building root commmand '{id}'...", rootData.Id);
        
        var commandBuilder = GetCommandBuilder(rootData);

        RootCommand = BuildCommand(commandBuilder, rootData);

        _logger?.LogInformation("Root commmand '{id}' was builded.", rootData.Id);
        
        ValidateCommandsParent();
    }

    /// <summary>
    /// Validate if every command has a parent, except root
    /// </summary>
    /// <exception cref="ClyshException">Throws an error if any command does not have a parent</exception>
    private void ValidateCommandsParent()
    {
        //Every command declared into file must has a parent, except root
        var everyCommandHasParent = Commands.Count == Data.Commands!.Count;

        if (everyCommandHasParent) return;
        
        _logger?.LogError("Some commands has a invalid parent.");

        var commandsWithoutParent = new List<string>();
        Data.Commands.ForEach(c =>
        {
            if (!Commands.Has(c.Id))
            {
                commandsWithoutParent.Add(c.Id);
                _logger?.LogDebug("The command '{id}' has a invalid parent.", c.Id);
            }
        });

        throw new ClyshException(string.Format(ClyshMessages.ErrorOnSetupCommandsParent, string.Join(';', commandsWithoutParent)));
    }

    private CommandData GetRootData()
    {
        try
        {
            _logger?.LogInformation("Getting root command data...");
            
            //Must have one root command. Throws an error if has any number different than one.
            var rootData = Data.Commands!.Single(x => x.Root);
            
            _logger?.LogInformation("Root command data was loaded.");

            return rootData;
        }
        catch (Exception)
        {
            _logger?.LogError("Has any value different than ONE root.");
            throw new ClyshException(ClyshMessages.ErrorOnSetupCommandsDuplicatedRoot);
        }
    }

    private void VerifyCommands()
    {
        _logger?.LogInformation("Verifying commands data...");
        
        if (Data.Commands != null && Data.Commands.Any())
        {
            _logger?.LogDebug("Commands list has some item.");
            VerifyDuplicatedCommands(Data.Commands);
            VerifyCommandsPattern(Data.Commands);
        }
        else
        {
            _logger?.LogDebug("Commands list is NULL or EMPTY.");
            throw new ClyshException(ClyshMessages.ErrorOnSetupCommandsLength);
        }

        _logger?.LogInformation("Commands data was verified.");
    }

    private void VerifyCommandsPattern(List<CommandData> dataCommands)
    {
        _logger?.LogInformation("Verifying commands pattern...");
        
        const string pattern = ClyshConstants.CommandPattern;
        var regex = new Regex(pattern);
        
        _logger?.LogDebug("Pattern to match: {pattern}", pattern);

        dataCommands.ForEach(command=>
        {
            if (regex.IsMatch(command.Id)) return;
            
            _logger?.LogError("Command '{commandId}' does not match the expected pattern.", command.Id);
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnValidateIdPattern, pattern, command.Id));
        });
    }

    private void VerifyDuplicatedCommands(List<CommandData> commands)
    {
        _logger?.LogInformation("Verifying duplicated command...");
        
        var hasNoDuplicatedCommandIds = commands.DistinctBy(x => x.Id).Count() == commands.Count;

        if (hasNoDuplicatedCommandIds)
        {
            _logger?.LogInformation("Duplicated command was not found.");
            return;
        }

        _logger?.LogError("Some command is duplicated.");
        _logger?.LogDebug("Preparing to show error with more details...");

        //Throw an error if has duplicated ids
        var duplicatedCommands = commands
            .GroupBy(x => x.Id)
            .Select(g => new { g.Key, Count = g.Count() })
            .Where(f => f.Count > 1);

        var ids = duplicatedCommands.Aggregate(string.Empty,
            (current, command) => $"{current}{command.Key},");

        ids = ids[..^1];
        
        _logger?.LogDebug("Detailed error message was done.");

        throw new ArgumentException(string.Format(ClyshMessages.ErrorOnSetupCommandsDuplicated, ids), nameof(commands));
    }

    /// <summary>
    /// Build a command from parent
    /// </summary>
    /// <param name="commandBuilder">The command builder</param>
    /// <param name="commandData">The command data</param>
    /// <exception cref="ClyshException"></exception>
    private ClyshCommand BuildCommand(ClyshCommandBuilder commandBuilder, CommandData commandData)
    {
        try
        {
            BuildCommandOptions(commandBuilder, commandData);
            BuildCommandSubcommands(commandBuilder, commandData);

            var command = commandBuilder.Build();
            
            _logger?.LogInformation("Commmand '{id}' was builded.", commandData.Id);
            
            Commands.Add(commandData.Id, command);

            return command;
        }
        catch (Exception e)
        {
            _logger?.LogError("Error while building commmand '{id}'.", commandData.Id);
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnCreateCommand, commandData.Id), e);
        }
    }

    private void BuildCommandSubcommands(ClyshCommandBuilder commandBuilder, CommandData commandData)
    {
        var subcommands = GetSubcommands(commandData);
        
        foreach (var subcommandId in subcommands)
        {
            _logger?.LogInformation("Building subcommand '{subcommandId}'...", subcommandId);
            
            try
            {
                //Throw an error if command id was not found
                var subcommandData = Data.Commands!.Single(x => x.Id == subcommandId);

                var subCommandBuilder = GetCommandBuilder(subcommandData);

                var subCommand = BuildCommand(subCommandBuilder, subcommandData);
                
                commandBuilder.SubCommand(subCommand);

                _logger?.LogInformation("Commmand '{id}' subcommand '{subcommandId}' was loaded.", commandData.Id, subcommandId);
            }
            catch (Exception e)
            {
                _logger?.LogError("Error while building subcommand '{subcommandId}'.", subcommandId);
                throw new ClyshException(string.Format(ClyshMessages.ErrorOnCreateSubCommand, subcommandId), e);
            }
        }
    }

    private ClyshCommandBuilder GetCommandBuilder(CommandData commandData)
    {
        _logger?.LogInformation("Building command '{id}'...", commandData.Id);
        
        var subCommandBuilder = new ClyshCommandBuilder();

        subCommandBuilder
            .Id(commandData.Id)
            .Description(commandData.Description);
        
        if (commandData.Abstract)
        {
            subCommandBuilder.MarkAsAbstract();
            _logger?.LogDebug("Command is ABSTRACT");
        }

        if (commandData.IgnoreParents)
        {
            subCommandBuilder.IgnoreParents();
            _logger?.LogDebug("Command IGNORE PARENTS actions");
        }

        return subCommandBuilder;
    }

    private List<string> GetSubcommands(CommandData commandData)
    {
        _logger?.LogInformation("Loading commmand '{id}' subcommands...", commandData.Id);
        
        var commandLevel = GetCommandLevel(commandData.Id);
        var nextLevel = commandLevel + 1;
        
        _logger?.LogDebug("Next level is {nextLevel}", nextLevel);

        var subcommands = Data.Commands!.Where(c =>
                c.Id.Contains(commandData.Id) &&
                GetCommandLevel(c.Id) == nextLevel)
            .Select(c => c.Id)
            .ToList();

        _logger?.LogInformation("Commmand '{id}' subcommands size is {size}.", commandData.Id, subcommands.Count);
        
        return subcommands;
    }

    private int GetCommandLevel(string commandId)
    {
        var level = commandId.Split(".", StringSplitOptions.RemoveEmptyEntries).Length - 1;
        _logger?.LogDebug("Commmand '{id}' level is {level}", commandId, level);
        
        return level;
    }

    private void BuildCommandOptions(ClyshCommandBuilder commandBuilder, CommandData commandData)
    {
        _logger?.LogInformation("Building commmand options...");

        var options = GetOptions(commandData);
        
        if (!options.Any())
            return;

        foreach (var optionData in options)
        {
            _logger?.LogInformation("Building commmand option '{optionId}'...", optionData.Id);

            var isGlobal = Data.Options?.Contains(optionData) ?? false;

            commandBuilder.Option(BuildOption(optionData, isGlobal));

            _logger?.LogInformation("Option was loaded to command.");
        }
        
        _logger?.LogInformation("Commmand options were builded.");
    }

    private List<OptionData> GetOptions(CommandData commandData)
    {
        var options = new List<OptionData>();

        if (commandData.Options != null)
        {
            options = options.Union(commandData.Options).ToList();
            _logger?.LogDebug("Commmand has {count} local options.", commandData.Options.Count);
        }
        else
            _logger?.LogInformation("Commmand does not have local options.");

        if (_commandGlobalOptions.ContainsKey(commandData.Id) || _commandGlobalOptions.ContainsKey("all"))
        {
            var globalOptions = ExtractGlobalOptions(commandData.Id);
            _logger?.LogDebug("Commmand has {count} global options.", globalOptions.Count);
            options = options.Union(globalOptions).ToList();
        }
        else
            _logger?.LogInformation("Commmand does not have global options.");

        return options;
    }

    private List<OptionData> ExtractGlobalOptions(string commandId)
    {
        IEnumerable<OptionData> options = new List<OptionData>();

        if (_commandGlobalOptions.ContainsKey(commandId)) 
            options = options.Union(_commandGlobalOptions[commandId]);

        if (_commandGlobalOptions.ContainsKey("all"))
            options = options.Union(_commandGlobalOptions["all"]);

        return options.ToList();
    }

    private ClyshOption BuildOption(
        OptionData optionData,
        bool globalOption = false)
    {
        _logger?.LogInformation("Building option '{id}'...", optionData.Id);

        var builder = new ClyshOptionBuilder();
        
        builder
            .Id(optionData.Id, optionData.Shortcut)
            .Description(optionData.Description);

        if (globalOption)
        {
            builder.MarkAsGlobal();
            _logger?.LogDebug("Option is global.");
        }

        BuildOptionGroup(builder, optionData);
        BuildOptionParameters(builder, optionData);

        var option =  builder.Build();
        
        _logger?.LogInformation("Option was builded.");

        return option;
    }

    private void BuildOptionParameters(ClyshOptionBuilder builder, OptionData option)
    {
        if (option.Parameters == null)
        {
            _logger?.LogDebug("Option does not have parameters.");
            return;
        }
        
        _logger?.LogDebug("Option has some parameters.");
        
        var parameterBuilder = new ClyshParameterBuilder();

        //Needs to order explicit by user input
        var parameters = option
            .Parameters
            .OrderBy(p => p.Order)
            .ToList();

        _logger?.LogDebug("Option parameters were ordered.");
        
        foreach (var p in parameters)
        {
            _logger?.LogInformation("Building option parameter '{id}'.", p.Id);

            if (p.Required)
            {
                parameterBuilder.MarkAsRequired();
                _logger?.LogDebug("Parameter is required.");
            }

            builder.Parameter(parameterBuilder
                .Id(p.Id)
                .Order(p.Order)
                .Pattern(p.Pattern)
                .Range(p.MinLength, p.MaxLength)
                .Build());
            
            _logger?.LogDebug("Parameter was builded.");
        }
    }

    private void BuildOptionGroup(ClyshOptionBuilder optionBuilder, OptionData option)
    {
        if (option.Group == null)
        {
            _logger?.LogInformation("Option does not have a group.");
            return;
        }
        
        var builder = new ClyshGroupBuilder();
        
        if (_groups.Has(option.Group))
        {
            _logger?.LogDebug("Option has a group.");
            _logger?.LogInformation("Option group is {group}. It was previously loaded.", option.Group);
        }
        else
        {
            _logger?.LogDebug("Option has a group.");
            _logger?.LogInformation("Option group is {group}. It is a new group.", option.Group);

            _groups.Add(builder
                .Id(option.Group)
                .Build());

            _logger?.LogInformation("Group was loaded.");
            _logger?.LogDebug("Groups has {count} items.", _groups.Count);
        }
        
        AssociateGroupToOption(optionBuilder, option);
    }

    private void AssociateGroupToOption(ClyshOptionBuilder builder, OptionData option)
    {
        _logger?.LogInformation("Associating group to option...");
        
        var group = _groups[option.Group!];
        group.Options.Add(option.Id);
        builder.Group(group);
        
        _logger?.LogInformation("Group was associated to option.");
    }

    private ClyshData Deserialize(DeserializerType deserializerType)
    {
        _logger?.LogInformation("Deserializer: {deserializerType}", deserializerType);
        
        var content = GetContentFromFilePath();
        
        _logger?.LogDebug("Running {deserializerType} deserializer...", deserializerType);

        var data = deserializerType == DeserializerType.Json ? JsonDeserialize(content): YamlDeserialize(content);
        
        _logger?.LogInformation("The content is deserialized.");

        return data;
    }

    private ClyshData JsonDeserialize(string content)
    {
        var data = JsonConvert.DeserializeObject<ClyshData>(content);
        
        if (data == null)
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnSetupLoadFileJson, _path));

        return data;
    }

    private string GetContentFromFilePath()
    {
        var content = _fs.File.ReadAllText(_path);
        
        _logger?.LogDebug("Content read.");

        return content;
    }

    private static ClyshData YamlDeserialize(string content)
    {
        var deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

        var data = deserializer.Deserialize<ClyshData>(content);
        
        return data;
    }

    private void Load()
    {
        try
        {
            _logger?.LogInformation("Loading data...");
            ExtractDataFromFileSystem();
            CreateGlobalOptionsFromData();
            CreateCommandsFromData();
            _logger?.LogInformation("Data was loaded.");
        }
        catch (ClyshException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnSetupLoad, _path), e);
        }
    }

    private void CreateGlobalOptionsFromData()
    {
        _logger?.LogInformation("Loading global options data...");
        
        if (Data.Options != null)
        {
            _logger?.LogDebug("Data.Options is NOT NULL");
            _logger?.LogDebug("Data.Options has {count} items.", Data.Options.Count);
            
            ProcessGlobalOptions(Data.Options);
            
            _logger?.LogInformation("Global options data were loaded.");
        }
        else
        {
            _logger?.LogDebug("Data.Options is NULL");
            _logger?.LogInformation("Global options were not found. There was nothing to do.");
        }
    }

    private void ProcessGlobalOptions(List<GlobalOptionData> globalOptionsData)
    {
        _logger?.LogInformation("Processing global options data...");
        
        foreach (var optionData in globalOptionsData)
            ProcessGlobalOption(optionData);
        
        _logger?.LogInformation("Global options data were processed.");
    }

    private void ProcessGlobalOption(GlobalOptionData optionData)
    {
        _logger?.LogInformation("Processing global option data '{id}'...", optionData.Id);
        
        var commands = GetCommands(optionData);

        foreach (var command in commands.Select(c => c.ToLower())) 
            ProcessOptionCommand(optionData, command);
    }

    private void ProcessOptionCommand(OptionData option, string command)
    {
        _logger?.LogInformation("Queueing option to command '{commandId}'...", command);
        
        if (!_commandGlobalOptions.ContainsKey(command))
        {
            _logger?.LogDebug("Adding command to {commandListName} list...", nameof(_commandGlobalOptions));
            
            _commandGlobalOptions.Add(command, new List<OptionData>
            {
                option
            });
            
            _logger?.LogDebug("Command was added to list.");
        }
        else
        {
            _logger?.LogDebug("Command was previously added to {commandListName} list.", nameof(_commandGlobalOptions));
            
            _commandGlobalOptions[command].Add(option);
        }

        _logger?.LogInformation("Global option is queued to command.");
    }

    private IEnumerable<string> GetCommands(GlobalOptionData optionData)
    {
        var commands = optionData.Commands;

        if (commands == null)
        {
            _logger?.LogDebug("Option commands list is NULL. An empty list will be created.");
            commands = new List<string>();
        }

        if (commands.Count == 0)
        {
            _logger?.LogDebug("Option commands list is EMPTY. All comands will be added.");
            commands.Add("all");
        }

        if (commands.Contains("all") && commands.Count > 1)
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnSetupGlobalOptionsAll, optionData.Id));
        
        return commands;
    }
}