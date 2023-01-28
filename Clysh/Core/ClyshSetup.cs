using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using Clysh.Core.Builder;
using Clysh.Data;
using Clysh.Helper;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using FileSystem = System.IO.Abstractions.FileSystem;

namespace Clysh.Core;

/// <summary>
/// The setup of <see cref="Clysh"/>
/// </summary>
public class ClyshSetup : IClyshSetup
{
    private const string MessageErrorOnBindAction = "Error on bind action with commandId '{0}'. The id was not found.";

    private const string MessageErrorOnLoad = "Error on load data from file path. Path: '{0}'";

    private const string MessageErrorOnCreateCommand = "Error on create command. Command: {0}";

    private const string MessageErrorOnCreateCommands = "Error on create commands from extracted data. Path: '{0}'";

    private const string MessageInvalidCommandsDuplicated =
        "Invalid commands: The id(s): {0} must be unique. Check your schema and try again.";

    private const string MessageInvalidCommandsDuplicatedRoot =
        "Invalid commands: Data must have one root command. Consider marking only one command with 'Root': true.";

    private const string MessageInvalidCommandsParent =
        "Invalid commands: The commands '{0}' does not have a parent. Check if all your commands has a valid parent.";

    private const string MessageInvalidCommandsLengthAtLeastOne =
        "Invalid commands: The data must contains at least one command.";

    private const string MessageInvalidCommandsNotFound =
        "Invalid commands: The commandId '{0}' was not found on commands data list.";

    private const string MessageInvalidFileExtension =
        "Invalid file: Only JSON (.json) and YAML (.yml or .yaml) files are supported. Path: '{0}'";

    private const string MessageInvalidFileJson =
        "Invalid file: The JSON deserialization results in null object. JSON file path: '{0}'";

    private const string MessageInvalidFilePath = "Invalid file: CLI data file was not found. Path: '{0}'";

    private readonly List<CommandData> _commandsData;
    private readonly string _path;

    public ClyshMap<IClyshCommand> Commands { get; }

    /// <summary>
    /// The <b>ClyshDataSetup</b> object
    /// </summary>
    /// <param name="fs">The file system object</param>
    /// <param name="path">The path of file. YAML or JSON format only</param>
    public ClyshSetup(string path, IFileSystem fs)
    {
        _path = path;
        _commandsData = new List<CommandData>();
        Commands = new ClyshMap<IClyshCommand>();
        Data = new ClyshData();
        Load(fs);
    }

    /// <summary>
    /// The <b>ClyshDataSetup</b> object
    /// </summary>
    /// <param name="path">The path of file. YAML or JSON format only</param>
    public ClyshSetup(string path) : this(path, new FileSystem())
    {
    }

    /// <summary>
    /// The CLI Root command
    /// </summary>
    public IClyshCommand RootCommand { get; private set; } = default!;

    /// <summary>
    /// The CLI Data
    /// </summary>
    public ClyshData Data { get; private set; }

    /// <summary>
    /// The CLI messages
    /// </summary>
    public Dictionary<string, string>? Messages { get; set; }

    /// <summary>
    /// Bind your command action
    /// </summary>
    /// <param name="commandId">The command id</param>
    /// <param name="action">The action to be executed</param>
    public void BindAction(string commandId, Action<IClyshCommand, IClyshView> action)
    {
        if (!Commands.Has(commandId))
            throw new ClyshException(string.Format(MessageErrorOnBindAction, commandId));

        var command = Commands[commandId];
        command.Action = action;
    }

    private void SetupMessages()
    {
        if (Data.Messages != null) Messages = Data.Messages;
    }

    private void ExtractDataFromFileSystem(IFileSystem fs)
    {
        if (!fs.File.Exists(_path))
            throw new ClyshException(string.Format(MessageInvalidFilePath, _path));

        var extension = fs.Path.GetExtension(_path);

        Data = extension switch
        {
            ".json" => JsonSerializer(fs),
            ".yml" => YamlSerializer(fs),
            ".yaml" => YamlSerializer(fs),
            _ => throw new ClyshException(string.Format(MessageInvalidFileExtension, _path))
        };
    }

    private void CreateCommandsFromData()
    {
        try
        {
            VerifyCommands();
            CreateCommands();
        }
        catch (ClyshException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ClyshException(MessageErrorOnCreateCommands, e);
        }
    }

    private void CreateCommands()
    {
        var rootData = GetRootData();

        var commandBuilder = new ClyshCommandBuilder();
        var root = commandBuilder
            .Id(rootData.Id)
            .Description(rootData.Description)
            .RequireSubcommand(rootData.RequireSubcommand)
            .Build();

        BuildCommand(root, rootData);

        RootCommand = root;
        
        ValidateCommandsParent();
    }

    /// <summary>
    /// Validate if every command has a parent, except root
    /// </summary>
    /// <exception cref="ClyshException">Throws an error if any command does not have a parent</exception>
    private void ValidateCommandsParent()
    {
        //Every command declared into file must has a parent, except root
        var everyCommandHasParent = Commands.Count == _commandsData.Count;

        if (everyCommandHasParent) return;

        var commandsWithoutParent = new List<string>();
        _commandsData.ForEach(c =>
        {
            if (!Commands.Has(c.Id))
                commandsWithoutParent.Add(c.Id);
        });

        throw new ClyshException(string.Format(MessageInvalidCommandsParent, string.Join(';', commandsWithoutParent)));
    }

    private CommandData GetRootData()
    {
        try
        {
            //Must have one root command. Throws an error if has any number different than one.
            return Data.Commands!.Single(x => x.Root);
        }
        catch (Exception)
        {
            throw new ClyshException(MessageInvalidCommandsDuplicatedRoot);
        }
    }

    private void VerifyCommands()
    {
        if (Data.Commands != null && Data.Commands.Any())
        {
            _commandsData.AddRange(Data.Commands);

            VerifyDuplicatedCommands(Data.Commands);
            VerifyCommandsPattern(Data.Commands);
        }
        else
            throw new ClyshException(MessageInvalidCommandsLengthAtLeastOne);
    }

    private static void VerifyCommandsPattern(List<CommandData> dataCommands)
    {
        const string pattern = ClyshConstants.CommandPattern;
        var regex = new Regex(pattern);

        dataCommands.ForEach(x=>
        {
            if (!regex.IsMatch(x.Id))
                throw new ClyshException(string.Format(ClyshMessages.MessageInvalidId, pattern, x.Id));
        });
    }

    private void BuildRootCommand()
    {
        
    }

    private static void VerifyDuplicatedCommands(List<CommandData> commands)
    {
        var hasNoDuplicatedCommandIds = commands.DistinctBy(x => x.Id).Count() == commands.Count;

        if (hasNoDuplicatedCommandIds)
            return;

        //Throw an error if has duplicated ids
        var duplicatedCommands = commands
            .GroupBy(x => x.Id)
            .Select(g => new { g.Key, Count = g.Count() })
            .Where(f => f.Count > 1);

        var ids = duplicatedCommands.Aggregate(string.Empty,
            (current, command) => $"{current}{command.Key},");

        ids = ids[..^1];

        throw new ArgumentException(string.Format(MessageInvalidCommandsDuplicated, ids), nameof(commands));
    }

    /// <summary>
    /// Build a command from parent
    /// </summary>
    /// <param name="command">The parent command</param>
    /// <param name="commandData">The parent command data</param>
    /// <exception cref="ClyshException"></exception>
    private void BuildCommand(IClyshCommand command, CommandData commandData)
    {
        try
        {
            Commands.Add(command.Id, command);
            BuildCommandGroups(command, commandData);
            BuildCommandOptions(command, commandData);
            BuildCommandSubcommands(command);
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(MessageErrorOnCreateCommand, command.Id), e);
        }
    }

    private void BuildCommandSubcommands(IClyshCommand command)
    {
        var subcommands = GetSubcommands(command);

        if (!subcommands.Any())
        {
            if (command.RequireSubcommand)
                throw new ClyshException(
                    "The command is configured to require subcommand. So subcommands cannot be null.");

            return;
        }

        foreach (var subcommandId in subcommands)
        {
            try
            {
                //Throw an error if command id was not found
                var subcommandData = _commandsData.SingleOrDefault(x => x.Id == subcommandId);

                if (subcommandData?.Id == null)
                    throw new ClyshException(string.Format(MessageInvalidCommandsNotFound, subcommandId));

                var commandBuilder = new ClyshCommandBuilder();

                var subCommand = commandBuilder
                    .Id(subcommandData.Id)
                    .Description(subcommandData.Description)
                    .RequireSubcommand(subcommandData.RequireSubcommand)
                    .Build();

                command.AddSubCommand(subCommand);

                BuildCommand(subCommand, subcommandData);
            }
            catch (Exception e)
            {
                throw new ClyshException($"Error on create subcommand. Subcommand: {subcommandId}", e);
            }
        }
    }

    private List<string> GetSubcommands(IClyshCommand command)
    {
        var commandLevel = GetCommandLevel(command.Id);
        var nextLevel = commandLevel + 1;

        var subcommands = _commandsData.Where(c =>
                c.Id.Contains(command.Id) &&
                GetCommandLevel(c.Id) == nextLevel)
            .Select(c => c.Id)
            .ToList();

        return subcommands;
    }

    private static int GetCommandLevel(string commandId)
    {
        return commandId.Split(".", StringSplitOptions.RemoveEmptyEntries).Length - 1;
    }

    private static void BuildCommandOptions(IClyshCommand command, CommandData commandData)
    {
        if (commandData.Options == null)
            return;

        var builder = new ClyshOptionBuilder();

        foreach (var option in commandData.Options)
            BuildOption(builder, command, option);
    }

    private static void BuildOption(ClyshOptionBuilder builder, IClyshCommand command, OptionData option)
    {
        builder
            .Id(option.Id, option.Shortcut)
            .Description(option.Description);

        BuildOptionGroup(builder, command, option);
        BuildOptionParameters(builder, option);

        command.AddOption(builder.Build());
    }

    private static void BuildOptionParameters(ClyshOptionBuilder builder, OptionData option)
    {
        if (option.Parameters == null)
            return;

        var parameterBuilder = new ClyshParameterBuilder();

        //Needs to order explicit by user input
        var parameters = option
            .Parameters
            .OrderBy(p => p.Order)
            .ToList();

        foreach (var p in parameters)
        {
            builder.Parameter(parameterBuilder
                .Id(p.Id)
                .Order(p.Order)
                .Pattern(p.Pattern)
                .Required(p.Required)
                .Range(p.MinLength, p.MaxLength)
                .Build());
        }
    }

    private static void BuildOptionGroup(ClyshOptionBuilder builder, IClyshCommand command, OptionData option)
    {
        if (option.Group == null) return;

        var group = command.Groups[option.Group];

        group.Options.Add(option.Id!);

        builder.Group(group);
    }

    private static void BuildCommandGroups(IClyshCommand command, CommandData commandData)
    {
        if (commandData.Groups == null)
            return;

        var groupBuilder = new ClyshGroupBuilder();

        foreach (var group in commandData.Groups)
            command.Groups.Add(groupBuilder.Id(group).Build());
    }

    private ClyshData JsonSerializer(IFileSystem fs)
    {
        var config = GetDataFromFilePath(fs);

        var data = JsonConvert.DeserializeObject<ClyshData>(config);

        if (data == null)
            throw new ClyshException(string.Format(MessageInvalidFileJson, _path));

        return data;
    }

    private string GetDataFromFilePath(IFileSystem fs)
    {
        return fs.File.ReadAllText(_path);
    }

    private ClyshData YamlSerializer(IFileSystem fs)
    {
        var data = GetDataFromFilePath(fs);

        var deserializer = new DeserializerBuilder().Build();

        return deserializer.Deserialize<ClyshData>(data);
    }

    /// <summary>
    /// Load CLI data from path and parse it
    /// </summary>
    /// <param name="fs">File system</param>
    private void Load(IFileSystem fs)
    {
        try
        {
            ExtractDataFromFileSystem(fs);
            SetupMessages();
            CreateCommandsFromData();
        }
        catch (ClyshException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(MessageErrorOnLoad, _path), e);
        }
    }
}