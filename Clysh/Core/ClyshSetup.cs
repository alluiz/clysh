using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using Clysh.Data;
using Clysh.Helper;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Clysh.Core;

public class ClyshSetup
{
    /// <summary>
    /// The path of file. YAML or JSON format only
    /// </summary>
    public string PathOfData { get; set; }

    /// <summary>
    /// The CLI Root command
    /// </summary>
    public ClyshCommand RootCommand { get; private set; }

    public ClyshData Data { get; set; }

    public const string InvalidPath = "Invalid path: CLI data file was not found.";
    public const string InvalidExtension = "Invalid extension. Only JSON (.json) and YAML (.yml or .yaml) files are supported.";
    public const string ErrorOnLoad = "Error on load data from file path.";
    public const string InvalidJson = "Invalid JSON: The deserialization results in null object.";
    public const string ErrorOnCreateRoot = "Error on create root or nested commands from extracted data.";
    public const string InvalidCommandsTheIdSMustBeUnique = "Invalid commands: The id(s): $0 must be unique check your schema and try again.";
    public const string InvalidCommandsLength = $"Invalid commands: The data must contains at once one command.";
    public const string InvalidCommandTheIdWasNotFound = $"Invalid commandId. The id: $0 was not found on commands data list.";

    private readonly Dictionary<string, ClyshCommand> commandsLoaded;
    private readonly List<ClyshCommandData> commandsData;
    private readonly IFileSystem fs;

    /// <summary>
    /// The <b>ClyshDataSetup</b> object
    /// </summary>
    /// <param name="fs">The file system object</param>
    /// <param name="pathOfData">The path of file. YAML or JSON format only</param>
    public ClyshSetup(IFileSystem fs, string pathOfData)
    {
        this.fs = fs;
        PathOfData = pathOfData;
        commandsData = new List<ClyshCommandData>();
        commandsLoaded = new Dictionary<string, ClyshCommand>();
        Data = new ClyshData();
        RootCommand = GetRootCommandFromFilePath(PathOfData);
    }

    /// <summary>
    /// The <b>ClyshDataSetup</b> object
    /// </summary>
    /// <param name="pathOfData">The path of file. YAML or JSON format only</param>
    [ExcludeFromCodeCoverage]
    public ClyshSetup(string pathOfData) : this(new FileSystem(), pathOfData)
    {
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
            throw new ClyshException(ErrorOnLoad, e);
        }
    }

    private void ExtractDataFromFile(string path)
    {
        if (!fs.File.Exists(path))
            throw new ArgumentException(InvalidPath, nameof(path));

        if (fs.Path.HasExtension(path))
        {
            var extension = fs.Path.GetExtension(path);

            switch (extension)
            {
                case ".json":
                    Data = JsonSerializer(path);
                    break;
                case ".yml":
                case ".yaml":
                    Data = YamlSerializer(path);
                    break;
                default:
                    throw new ArgumentException(InvalidExtension, nameof(path));
            }
        }
        else
            throw new ArgumentException(InvalidExtension, nameof(path));
    }

    public void MakeAction(string commandId, Action<IClyshCommand, ClyshMap<ClyshOption>, IClyshView> action)
    {
        var command = commandsLoaded[commandId];
            
        command.Action = action;
    }

    public bool IsReadyToProduction()
    {
        var allHasActions = commandsLoaded.All(x => x.Value.Action != null);
        var allHasDescription = commandsLoaded.All(x => x.Value.Description != null);

        return allHasActions && allHasDescription;
    }

    private ClyshCommand CreateRootFromExtractedData()
    {
        try
        {
            if (Data.Commands == null)
                throw new ArgumentException(InvalidCommandsLength, nameof(Data));
                
            if (Data.Commands.DistinctBy(x => x.Id).Count() != Data.Commands.Count)
                HandleIdsError(Data.Commands);

            var rootData = Data.Commands.SingleOrDefault(x => x.Root);

            if (rootData == null)
                throw new ArgumentNullException(nameof(Data), "Data must have at least one root command.");

            commandsData.AddRange(Data.Commands);

            var commandBuilder = new ClyshCommandBuilder();
            var root = commandBuilder
                .Id(rootData.Id)
                .Description(rootData.Description)
                .Build();
                
            commandsLoaded.Add(root.Id, root);

            LoadCommands(root, rootData);

            return root;
        }
        catch (Exception e)
        {
            throw new ClyshException(ErrorOnCreateRoot, e);
        }
    }

    private static void HandleIdsError(List<ClyshCommandData> commands)
    {
        var duplicatedCommands = commands.GroupBy(x => x.Id)
            .Select(g => new {g.Key, Count = g.Count()}).Where(f => f.Count > 1);

        var ids = duplicatedCommands.Aggregate(string.Empty, (current, command) => $"{current}{command.Key},");

        ids = ids[..^1];

        throw new ArgumentException(
            InvalidCommandsTheIdSMustBeUnique.Replace("$0", ids),
            nameof(commands));
    }

    private void LoadCommands(IClyshCommand command, ClyshCommandData commandData)
    {
        if (commandData.Groups != null)
        {
            foreach (var group in commandData.Groups)
            {
                var groupBuilder = new ClyshGroupBuilder();
                command.Groups.Add(groupBuilder
                    .Id(group)
                    .Build());
            }
        }
        
        if (commandData.Options != null)
        {
            foreach (var option in commandData.Options)
            {
                var optionBuilder = new ClyshOptionBuilder();

                optionBuilder
                    .Id(option.Id ?? throw new InvalidOperationException())
                    .Description(option.Description ?? throw new InvalidOperationException())
                    .Shortcut(option.Shortcut);

                if (option.Group != null)
                {
                    if (command.Groups == null || !command.Groups.ContainsKey(option.Group))
                        throw new InvalidOperationException($"Invalid group '{option.Group}'. You need to add it to 'Groups' field of command.");
                    
                    var group = command.Groups[option.Group];
                    optionBuilder
                        .Group(group);
                }
                
                if (option.Parameters != null)
                {
                    var parameters = new ClyshParameters();
                    option.Parameters.ForEach(x =>
                        parameters.Add(new ClyshParameter(x.Id, x.MinLength, x.MaxLength, x.Required, x.Pattern)));
                    optionBuilder.Parameters(parameters);
                }
                
                var optionBuilded = optionBuilder.Build();
                command.AddOption(optionBuilded);
            }
        }

        if (commandData.Children != null)
        {
            foreach (var childrenCommandId in commandData.Children)
            {
                var childrenCommandData =
                    commandsData.SingleOrDefault(x => x.Id == childrenCommandId);

                if (childrenCommandData == null)
                    throw new InvalidOperationException(
                        InvalidCommandTheIdWasNotFound.Replace("$0", childrenCommandId));

                VerifyParentRecursivity(command, childrenCommandData);

                var alreadyLoaded = commandsLoaded.ContainsKey(childrenCommandData.Id);

                var commandBuilder = new ClyshCommandBuilder();
                    
                var children = alreadyLoaded
                    ? commandsLoaded[childrenCommandData.Id]
                    : commandBuilder
                        .Id(childrenCommandData.Id)
                        .Description(childrenCommandData.Description)
                        .Build();

                command.AddChild(children);

                if (!alreadyLoaded)
                {
                    LoadCommands(children, childrenCommandData);
                    commandsLoaded.Add(children.Id, children);
                }
            }
        }
    }

    private static void VerifyParentRecursivity(IClyshCommand command, ClyshCommandData childrenCommandData)
    {
        var tree = childrenCommandData.Id;
        var commandCheck = command;

        while (commandCheck != null)
        {
            if (childrenCommandData.Id.Equals(commandCheck.Id))
                throw new InvalidOperationException("Command Error: The command '$0' must not be children of itself: $1"
                    .Replace("$0", childrenCommandData.Id)
                    .Replace("$1", $"{commandCheck.Id}>{tree}")
                );

            tree = $"{commandCheck.Id}>{tree}";
            commandCheck = commandCheck.Parent;
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