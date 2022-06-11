using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
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

    private readonly ClyshMap<ClyshCommand> commandsLoaded;
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
        commandsLoaded = new ClyshMap<ClyshCommand>();
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

            return CreateRootFromExtractedData();
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

    private ClyshCommand CreateRootFromExtractedData()
    {
        try
        {
            if (Data.Commands == null || !Data.Commands.Any())
                throw new ArgumentException(InvalidCommandsLength, nameof(Data));

            if (Data.Commands.DistinctBy(x => x.Id).Count() != Data.Commands.Count)
                HandleDuplicatedIdsError(Data.Commands);

            var rootData = Data.Commands.SingleOrDefault(x => x.Root);

            if (rootData == null)
                throw new ArgumentNullException(nameof(Data), OneCommandWithRootTrue);

            commandsData.AddRange(Data.Commands);

            var root = BuildRootCommand(rootData);

            LoadCommands(root, rootData);

            return root;
        }
        catch (Exception e)
        {
            throw new ClyshException(ErrorOnCreateRoot, e);
        }
    }

    private ClyshCommand BuildRootCommand(ClyshCommandData rootData)
    {
        var commandBuilder = new ClyshCommandBuilder();
        var root = commandBuilder
            .Id(rootData.Id)
            .Description(rootData.Description)
            .Build();

        commandsLoaded.Add(root.Id, root);

        return root;
    }

    private static void HandleDuplicatedIdsError(List<ClyshCommandData> commands)
    {
        var duplicatedCommands = commands.GroupBy(x => x.Id)
            .Select(g => new { g.Key, Count = g.Count() }).Where(f => f.Count > 1);

        var ids = duplicatedCommands.Aggregate(string.Empty, (current, command) => $"{current}{command.Key},");

        ids = ids[..^1];

        throw new ArgumentException(string.Format(InvalidCommandsTheIdSMustBeUnique, ids), nameof(commands));
    }

    private void LoadCommands(IClyshCommand command, ClyshCommandData commandData)
    {
        BuildCommandGroups(command, commandData);
        BuildCommandOptions(command, commandData);
        BuildCommandSubcommands(command, commandData);
    }

    private void BuildCommandSubcommands(IClyshCommand command, ClyshCommandData commandData)
    {
        command.RequireSubcommand = commandData.RequireSubcommand;
        
        if (commandData.SubCommands == null) return;

        foreach (var childrenCommandId in commandData.SubCommands)
        {
            var childrenCommandData =
                commandsData.SingleOrDefault(x => x.Id == childrenCommandId);

            if (childrenCommandData == null)
                throw new InvalidOperationException(
                    InvalidCommandTheIdWasNotFound.Replace("$0", childrenCommandId));

            var alreadyLoaded = commandsLoaded.ContainsKey(childrenCommandData.Id);

            var commandBuilder = new ClyshCommandBuilder();

            var children = alreadyLoaded
                ? commandsLoaded[childrenCommandData.Id]
                : commandBuilder
                    .Id(childrenCommandData.Id)
                    .Description(childrenCommandData.Description)
                    .Build();

            command.AddSubCommand(children);

            if (!alreadyLoaded)
            {
                LoadCommands(children, childrenCommandData);
                commandsLoaded.Add(children.Id, children);
            }
        }
    }

    private static void BuildCommandOptions(IClyshCommand command, ClyshCommandData commandData)
    {
        if (commandData.Options == null) return;

        foreach (var option in commandData.Options)
        {
            var optionBuilder = new ClyshOptionBuilder();

            optionBuilder
                .Id(option.Id,
                    option.Shortcut)
                .Description(option.Description);

            if (option.Group != null)
            {
                if (command.Groups == null || !command.Groups.ContainsKey(option.Group))
                    throw new InvalidOperationException(
                        $"Invalid group '{option.Group}'. You need to add it to 'Groups' field of command.");

                var group = command.Groups[option.Group];
                optionBuilder
                    .Group(group);
            }

            if (option.Parameters != null)
            {
                var parameterBuilder = new ClyshParameterBuilder();
                var parameters = new ClyshParameters();
                option.Parameters.ForEach(x => parameters.Add(
                    parameterBuilder
                        .Id(x.Id)
                        .Pattern(x.Pattern)
                        .Required(x.Required)
                        .Range(x.MinLength, x.MaxLength)
                        .Build()));
                optionBuilder.Parameters(parameters);
            }

            var optionBuilded = optionBuilder.Build();
            command.AddOption(optionBuilded);
        }
    }

    private static void BuildCommandGroups(IClyshCommand command, ClyshCommandData commandData)
    {
        if (commandData.Groups == null) return;

        foreach (var group in commandData.Groups)
        {
            var groupBuilder = new ClyshGroupBuilder();
            command.Groups.Add(groupBuilder
                .Id(group)
                .Build());
        }
    }

    private ClyshData JsonSerializer(string path)
    {
        var config = GetDataFromFilePath(path);

        var data = JsonConvert.DeserializeObject<ClyshData>(config);

        if (data == null)
            throw new InvalidOperationException(InvalidJson);

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