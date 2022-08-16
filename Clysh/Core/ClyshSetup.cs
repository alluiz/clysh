using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
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
public class ClyshSetup
{
    private const string InvalidPath = "Invalid path: CLI data file was not found. Path: '{0}'";

    private const string InvalidExtension =
        "Invalid extension. Only JSON (.json) and YAML (.yml or .yaml) files are supported. Path: '{0}'";

    private const string ErrorOnLoad = "Error on load data from file path. Path: '{0}'";

    private const string InvalidJson =
        "Invalid JSON: The deserialization results in null object. JSON file path: '{0}'";

    private const string ErrorOnCreateRoot = "Error on create root or nested commands from extracted data. Path: '{0}'";

    private const string InvalidCommandsTheIdSMustBeUnique =
        "Invalid commands: The id(s): {0} must be unique check your schema and try again.";

    private const string InvalidCommandsLength = "Invalid commands: The data must contains at once one command.";

    private const string InvalidCommandTheIdWasNotFound =
        "Invalid commandId. The id: {0} was not found on commands data list.";

    private const string OneCommandWithRootTrue =
        "Data must have one root command. Consider marking only one command with 'Root': true.";

    private const string ErrorOnCreateCommand = "Error on create command. Command: {0}";

    private readonly List<ClyshCommandData> commandsData;

    private readonly ClyshMap<IClyshCommand> commandsLoaded;
    private readonly IFileSystem fs;
    private readonly string path;

    /// <summary>
    /// The <b>ClyshDataSetup</b> object
    /// </summary>
    /// <param name="fs">The file system object</param>
    /// <param name="path">The path of file. YAML or JSON format only</param>
    public ClyshSetup(string path, IFileSystem fs)
    {
        this.path = path;
        this.fs = fs;
        commandsData = new List<ClyshCommandData>();
        commandsLoaded = new ClyshMap<IClyshCommand>();
        Data = new ClyshData();
    }

    /// <summary>
    /// The <b>ClyshDataSetup</b> object
    /// </summary>
    /// <param name="path">The path of file. YAML or JSON format only</param>
    [ExcludeFromCodeCoverage]
    public ClyshSetup(string path) : this(path, new FileSystem())
    {
    }

    /// <summary>
    /// The CLI Root command
    /// </summary>
    public ClyshCommand RootCommand { get; private set; } = default!;

    /// <summary>
    /// The CLI Data
    /// </summary>
    public ClyshData Data { get; private set; }

    /// <summary>
    /// The CLI messages
    /// </summary>
    public Dictionary<string,string>? Messages { get; set; }

    /// <summary>
    /// Bind your command action
    /// </summary>
    /// <param name="commandId">The command id</param>
    /// <param name="action">The action to be executed</param>
    public void BindAction(string commandId, Action<IClyshCommand, IClyshView> action)
    {
        var command = commandsLoaded[commandId];
        command.Action = action;
    }

    private ClyshCommand GetRootCommandFromFilePath()
    {
        try
        {
            ExtractDataFromFile();
            SetupMessages();
            return CreateRootFromData();
        }
        catch (ClyshException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ErrorOnLoad, path), e);
        }
    }

    private void SetupMessages()
    {
        if (Data.Messages != null)
        {
            Messages = Data.Messages;
        }
    }

    private void ExtractDataFromFile()
    {
        if (!fs.File.Exists(path))
            throw new ClyshException(string.Format(InvalidPath, path));

        var extension = fs.Path.GetExtension(path);

        Data = extension switch
        {
            ".json" => JsonSerializer(),
            ".yml" => YamlSerializer(),
            ".yaml" => YamlSerializer(),
            _ => throw new ClyshException(string.Format(InvalidExtension, path))
        };
    }

    private ClyshCommand CreateRootFromData()
    {
        try
        {
            VerifyCommands();
            VerifyDuplicatedIds(Data.Commands!);
            return Root();
        }
        catch (ClyshException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ClyshException(ErrorOnCreateRoot, e);
        }
    }

    private ClyshCommand Root()
    {
        //Return root command data
        var rootData = GetRootData();

        //Build root with data
        var root = BuildRootCommand(rootData);

        BuildCommand(root, rootData);

        if (commandsLoaded.Count != commandsData.Count)
            throw new ClyshException(
                "The commands loaded size is different than commands declared in file. Check if all your commands has a valid parent.");

        return root;
    }

    private ClyshCommandData GetRootData()
    {
        try
        {
            //Must have one root command. Throw an error if has any number different than one.
            var rootData = Data.Commands!.SingleOrDefault(x => x.Root);

            if (rootData == null)
                throw new ClyshException(OneCommandWithRootTrue);

            return rootData;
        }
        catch (Exception e)
        {
            throw new ClyshException(OneCommandWithRootTrue, e);
        }
    }

    private void VerifyCommands()
    {
        //Throw an error if has no command
        if (Data.Commands == null || !Data.Commands.Any())
            throw new ClyshException(InvalidCommandsLength);

        commandsData.AddRange(Data.Commands);
    }

    private ClyshCommand BuildRootCommand(ClyshCommandData rootData)
    {
        var commandBuilder = new ClyshCommandBuilder();
        var root = commandBuilder
            .Id(rootData.Id)
            .Description(rootData.Description)
            .RequireSubcommand(rootData.RequireSubcommand)
            .Build();

        return root;
    }

    private static void VerifyDuplicatedIds(List<ClyshCommandData> commands)
    {
        if (commands.DistinctBy(x => x.Id).Count() == commands.Count)
            return;

        //Throw an error if has duplicated ids
        var duplicatedCommands = commands
            .GroupBy(x => x.Id)
            .Select(g => new { g.Key, Count = g.Count() })
            .Where(f => f.Count > 1);

        var ids = duplicatedCommands.Aggregate(string.Empty,
            (current, command) => $"{current}{command.Key},");

        ids = ids[..^1];

        throw new ArgumentException(string.Format(InvalidCommandsTheIdSMustBeUnique, ids), nameof(commands));
    }

    private void BuildCommand(IClyshCommand command, ClyshCommandData commandData)
    {
        try
        {
            commandsLoaded.Add(command.Id, command);
            BuildCommandGroups(command, commandData);
            BuildCommandOptions(command, commandData);
            BuildCommandSubcommands(command);
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ErrorOnCreateCommand, command.Id), e);
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
                var subcommandData = commandsData.SingleOrDefault(x => x.Id == subcommandId);

                if (subcommandData?.Id == null)
                    throw new ClyshException(string.Format(InvalidCommandTheIdWasNotFound, subcommandId));

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
        //Declare two vars only for readable purpouse
        var commandLevel = GetCommandLevel(command.Id);
        var nextLevel = commandLevel + 1;

        var subcommands = commandsData.Where(x =>
                x.Id.Contains(command.Id) &&
                GetCommandLevel(x.Id) == nextLevel)
            .Select(c => c.Id)
            .ToList();

        return subcommands;
    }

    private static int GetCommandLevel(string commandId)
    {
        return commandId.Split(".").Length - 1;
    }

    private static void BuildCommandOptions(IClyshCommand command, ClyshCommandData commandData)
    {
        if (commandData.Options == null)
            return;

        var builder = new ClyshOptionBuilder();

        foreach (var option in commandData.Options)
            BuildOption(builder, command, option);
    }

    private static void BuildOption(ClyshOptionBuilder builder, IClyshCommand command, ClyshOptionData option)
    {
        builder
            .Id(option.Id, option.Shortcut)
            .Description(option.Description);

        BuildOptionGroup(builder, command, option);
        BuildOptionParameters(builder, option);

        command.AddOption(builder.Build());
    }

    private static void BuildOptionParameters(ClyshOptionBuilder builder, ClyshOptionData option)
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

    private static void BuildOptionGroup(ClyshOptionBuilder builder, IClyshCommand command, ClyshOptionData option)
    {
        if (option.Group == null) return;

        var group = command.Groups[option.Group];

        group.Options.Add(option.Id!);

        builder.Group(group);
    }

    private static void BuildCommandGroups(IClyshCommand command, ClyshCommandData commandData)
    {
        if (commandData.Groups == null)
            return;

        var groupBuilder = new ClyshGroupBuilder();

        foreach (var group in commandData.Groups)
        {
            command
                .Groups
                .Add(groupBuilder
                    .Id(group)
                    .Build());
        }
    }

    private ClyshData JsonSerializer()
    {
        var config = GetDataFromFilePath();

        var data = JsonConvert.DeserializeObject<ClyshData>(config);

        if (data == null)
            throw new ClyshException(string.Format(InvalidJson, path));

        return data;
    }

    private string GetDataFromFilePath()
    {
        return fs.File.ReadAllText(path);
    }

    private ClyshData YamlSerializer()
    {
        var data = GetDataFromFilePath();

        var deserializer = new DeserializerBuilder().Build();

        return deserializer.Deserialize<ClyshData>(data);
    }

    /// <summary>
    /// Load CLI data from path and parse it
    /// </summary>
    public void Load()
    {
        RootCommand = GetRootCommandFromFilePath();
    }
}