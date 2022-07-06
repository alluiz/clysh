using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
using Clysh.Core.Builder;
using Clysh.Data;
using Clysh.Helper;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Clysh.Core;

/// <summary>
/// The setup of <see cref="Clysh"/>
/// </summary>
public class ClyshSetup
{
    /// <summary>
    /// The CLI Root command
    /// </summary>
    public ClyshCommand RootCommand { get; }

    /// <summary>
    /// The CLI Data
    /// </summary>
    public ClyshData Data { get; private set; }

    private const string InvalidPath = "Invalid path: CLI data file was not found.";

    private const string InvalidExtension =
        "Invalid extension. Only JSON (.json) and YAML (.yml or .yaml) files are supported.";

    private const string ErrorOnLoad = "Error on load data from file path. Path: '{0}'";
    private const string InvalidJson = "Invalid JSON: The deserialization results in null object.";
    private const string ErrorOnCreateRoot = "Error on create root or nested commands from extracted data.";

    private const string InvalidCommandsTheIdSMustBeUnique =
        "Invalid commands: The id(s): {0} must be unique check your schema and try again.";

    private const string InvalidCommandsLength = "Invalid commands: The data must contains at once one command.";

    private const string InvalidCommandTheIdWasNotFound =
        "Invalid commandId. The id: $0 was not found on commands data list.";

    private const string OneCommandWithRootTrue =
        "Data must have one root command. Consider marking only one command with 'Root': true.";

    private const string InvalidGroup =
        "Invalid group '{0}'. You need to add it to 'Groups' field of command. Option: '{1}'";

    private readonly ClyshMap<IClyshCommand> commandsLoaded;
    private readonly List<ClyshCommandData> commandsData;
    private readonly IFileSystem fs;

    /// <summary>
    /// The <b>ClyshDataSetup</b> object
    /// </summary>
    /// <param name="fs">The file system object</param>
    /// <param name="pathOfData">The path of file. YAML or JSON format only</param>
    public ClyshSetup(string pathOfData, IFileSystem fs)
    {
        this.fs = fs;
        commandsData = new List<ClyshCommandData>();
        commandsLoaded = new ClyshMap<IClyshCommand>();
        Data = new ClyshData();
        RootCommand = GetRootCommandFromFilePath(pathOfData);
    }

    /// <summary>
    /// The <b>ClyshDataSetup</b> object
    /// </summary>
    /// <param name="pathOfData">The path of file. YAML or JSON format only</param>
    [ExcludeFromCodeCoverage]
    public ClyshSetup(string pathOfData) : this(pathOfData, new FileSystem())
    {
    }

    /// <summary>
    /// Make your custom command action
    /// </summary>
    /// <param name="commandId">The command id</param>
    /// <param name="action">The action to be executed</param>
    public void MakeAction(string commandId, Action<IClyshCommand, ClyshMap<ClyshOption>, IClyshView> action)
    {
        var command = commandsLoaded[commandId];

        command.Action = action;
    }

    private ClyshCommand GetRootCommandFromFilePath(string path)
    {
        try
        {
            ExtractDataFromFile(path);

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

    private void ExtractDataFromFile(string path)
    {
        if (!fs.File.Exists(path))
            throw new ArgumentException(InvalidPath, nameof(path));

        var extension = fs.Path.GetExtension(path);

        Data = extension switch
        {
            ".json" => JsonSerializer(path),
            ".yml" => YamlSerializer(path),
            ".yaml" => YamlSerializer(path),
            _ => throw new ArgumentException(InvalidExtension, nameof(path))
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

        return root;
    }

    private ClyshCommandData GetRootData()
    {
        //Must have one root command. Throw an error if has any number different than one.
        var rootData = Data.Commands!.SingleOrDefault(x => x.Root);

        if (rootData == null)
            throw new ArgumentNullException(nameof(Data), OneCommandWithRootTrue);

        return rootData;
    }

    private void VerifyCommands()
    {
        //Throw an error if has no command
        if (Data.Commands == null || !Data.Commands.Any())
            throw new ArgumentException(InvalidCommandsLength, nameof(Data));

        commandsData.AddRange(Data.Commands);
    }

    private ClyshCommand BuildRootCommand(ClyshCommandData rootData)
    {
        var commandBuilder = new ClyshCommandBuilder();
        var root = commandBuilder
            .Id(rootData.Id)
            .Description(rootData.Description)
            .Build();

        return root;
    }

    private static void VerifyDuplicatedIds(List<ClyshCommandData> commands)
    {
        //Throw an error if has duplicated ids
        if (commands.DistinctBy(x => x.Id).Count() != commands.Count)
        {
            var duplicatedCommands = commands.GroupBy(x => x.Id)
                .Select(g => new { g.Key, Count = g.Count() }).Where(f => f.Count > 1);

            var ids = duplicatedCommands.Aggregate(string.Empty, (current, command) => $"{current}{command.Key},");

            ids = ids[..^1];

            throw new ArgumentException(string.Format(InvalidCommandsTheIdSMustBeUnique, ids), nameof(commands));
        }
    }

    private void BuildCommand(IClyshCommand command, ClyshCommandData commandData)
    {
        commandsLoaded.Add(command.Id, command);
        BuildCommandGroups(command, commandData);
        BuildCommandOptions(command, commandData);
        BuildCommandSubcommands(command, commandData);
    }

    private void BuildCommandSubcommands(IClyshCommand command, ClyshCommandData commandData)
    {
        command.RequireSubcommand = commandData.RequireSubcommand;

        if (commandData.SubCommands == null)
        {
            if (command.RequireSubcommand)
                throw new ClyshException(
                    "The command is configured to require subcommand. So subcommands cannot be null.");

            return;
        }

        foreach (var childrenCommandId in commandData.SubCommands)
        {
            //Throw an error if command id was not found
            var childrenCommandData = commandsData.SingleOrDefault(x => x.Id == childrenCommandId);

            if (childrenCommandData?.Id == null)
                throw new ClyshException(
                    InvalidCommandTheIdWasNotFound.Replace("$0", childrenCommandId));

            var commandBuilder = new ClyshCommandBuilder();

            var child = commandBuilder
                .Id(childrenCommandData.Id)
                .Description(childrenCommandData.Description)
                .Build();

            command.AddSubCommand(child);
            
            BuildCommand(child, childrenCommandData);
        }
    }

    private static void BuildCommandOptions(IClyshCommand command, ClyshCommandData commandData)
    {
        if (commandData.Options == null) return;

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
        if (option.Parameters == null) return;

        var parameterBuilder = new ClyshParameterBuilder();

        if (option.Parameters.Any(x => x.Order == null))
            throw new ClyshException("All parameters must have explicit order.");

        //Needs to order explicit by user input
        var parameters = option.Parameters
            .OrderBy(p => p.Order)
            .ToList();

        //Used to check if optional parameter come before required
        var hasProvidedOptionalBefore = false;

        foreach (var p in parameters)
        {
            if (p.Required && hasProvidedOptionalBefore)
                throw new ClyshException(
                    "Invalid order. The required parameters must come first than optional parameters. Check the order.");
            
            hasProvidedOptionalBefore = !p.Required;

            builder.Parameter(parameterBuilder
                .Id(p.Id)
                .Pattern(p.Pattern)
                .Required(p.Required)
                .Range(p.MinLength, p.MaxLength)
                .Build());
        }
    }

    private static void BuildOptionGroup(ClyshOptionBuilder builder, IClyshCommand command, ClyshOptionData option)
    {
        if (option.Group == null) return;

        if (!command.Groups.Has(option.Group))
            throw new ClyshException(string.Format(InvalidGroup, option.Group, option.Id));

        var group = command.Groups[option.Group];

        builder.Group(group);
    }

    private static void BuildCommandGroups(IClyshCommand command, ClyshCommandData commandData)
    {
        if (commandData.Groups == null) return;

        var groupBuilder = new ClyshGroupBuilder();

        foreach (var group in commandData.Groups)
        {
            command.Groups.Add(
                groupBuilder
                    .Id(group)
                    .Build());
        }
    }

    private ClyshData JsonSerializer(string path)
    {
        var config = GetDataFromFilePath(path);

        var data = JsonConvert.DeserializeObject<ClyshData>(config);

        if (data == null)
            throw new ClyshException(InvalidJson);

        return data;
    }

    private string GetDataFromFilePath(string path)
    {
        return fs.File.ReadAllText(path);
    }

    private ClyshData YamlSerializer(string path)
    {
        var data = GetDataFromFilePath(path);

        var deserializer = new DeserializerBuilder().Build();

        return deserializer.Deserialize<ClyshData>(data);
    }
}