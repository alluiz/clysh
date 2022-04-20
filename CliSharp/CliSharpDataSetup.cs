using System.IO.Abstractions;
using CliSharp.Data;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace CliSharp
{
    public class CliSharpDataSetup
    {
        /// <summary>
        /// The path of file. YAML or JSON format only
        /// </summary>
        public string PathOfData { get; set; }

        /// <summary>
        /// The CLI Root command
        /// </summary>
        public ICliSharpCommand RootCommand { get; private set; }

        public CliSharpData Data { get; set; }

        public const string InvalidPath = "Invalid path: CLI data file was not found.";
        public const string InvalidExtension = "Invalid extension. Only JSON (.json) and YAML (.yml or .yaml) files are supported.";
        public const string ErrorOnLoad = "Error on load data from file path.";
        public const string InvalidJson = "Invalid JSON: The deserialization results in null object.";
        public const string ErrorOnCreateRoot = "Error on create root from extracted data.";
        public const string InvalidCommandsTheIdSMustBeUnique = "Invalid commands: The id(s): $0 must be unique check your schema and try again.";
        public const string InvalidCommandsLength = $"Invalid commands: The data must contains at once one command.";
        public const string InvalidCommandTheIdWasNotFound = $"Invalid commandId. The id: $0 was not found on commands data list.";

        private readonly Dictionary<string, ICliSharpCommand> commandsLoaded;
        private readonly List<CliSharpCommandData> commandsData;
        private readonly IFileSystem fs;

        /// <summary>
        /// The <b>CliSharpDataSetup</b> object
        /// </summary>
        /// <param name="fs">The file system object</param>
        /// <param name="pathOfData">The path of file. YAML or JSON format only</param>
        public CliSharpDataSetup(IFileSystem fs, string pathOfData)
        {
            this.fs = fs;
            this.PathOfData = pathOfData;
            this.commandsData = new();
            this.commandsLoaded = new();
            this.Data = new CliSharpData();
            this.RootCommand = GetRootCommandFromFilePath(PathOfData);
        }

        /// <summary>
        /// The <b>CliSharpDataSetup</b> object
        /// </summary>
        /// <param name="pathOfData">The path of file. YAML or JSON format only</param>
        public CliSharpDataSetup(string pathOfData) : this(new FileSystem(), pathOfData)
        {
        }


        private ICliSharpCommand GetRootCommandFromFilePath(string path)
        {
            try
            {
                ExtractDataFromFile(path);

                return CreateRootFromExtractedData();
            }
            catch (Exception e)
            {
                throw new ArgumentException(ErrorOnLoad, nameof(path), e);
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

        public void SetCommandAction(string commandId, Action<CliSharpMap<CliSharpOption>, ICliSharpView> action)
        {
            ICliSharpCommand command = commandsLoaded[commandId];
            
            command.Action = action;
        }

        private ICliSharpCommand CreateRootFromExtractedData()
        {
            try
            {
                if (Data.CommandsData.DistinctBy(x => x.Id).Count() != Data.CommandsData.Count)
                    HandleIdsError(Data.CommandsData);
                else if (Data.CommandsData.Count == 0)
                    throw new ArgumentException(InvalidCommandsLength, nameof(Data));

                CliSharpCommandData? rootData = Data.CommandsData.SingleOrDefault(x => x.Root);

                if (rootData == null)
                    throw new ArgumentNullException(nameof(Data), "Data must have at least one root command.");

                commandsData.AddRange(Data.CommandsData);

                ICliSharpCommand root = CliSharpCommand.Create(rootData.Id, rootData.Description);
                commandsLoaded.Add(root.Id, root);
                
                LoadCommands(root, rootData);

                return root;
            }
            catch (Exception e)
            {
                throw new ArgumentNullException(ErrorOnCreateRoot, e);
            }
        }

        private static void HandleIdsError(List<CliSharpCommandData> commands)
        {
            var duplicatedCommands = commands.GroupBy(x => x.Id)
                .Select(g => new {g.Key, Count = g.Count()}).Where(f => f.Count > 1);

            string ids = duplicatedCommands.Aggregate(string.Empty, (current, command) => $"{current}{command.Key},");

            if (ids != null)
            {
                ids = ids[..^1];

                throw new ArgumentException(
                    InvalidCommandsTheIdSMustBeUnique.Replace("$0", ids),
                    nameof(commands));
            }
        }

        private void LoadCommands(ICliSharpCommand command, CliSharpCommandData commandData)
        {
            if (commandData.OptionsData != null)
            {
                foreach (CliSharpOptionData option in commandData.OptionsData)
                {
                    if (option.ParametersData != null)
                    {
                        CliSharpParameters parameters = CliSharpParameters.Create(option.ParametersData.Select(x =>
                                new CliSharpParameter(x.Id, x.MinLength, x.MaxLength, x.Required, x.Pattern))
                            .ToArray());
                        command.AddOption(option.Id, option.Description, option.Shortcut, parameters);
                    }
                    else
                    {
                        command.AddOption(option.Id, option.Description, option.Shortcut);
                    }
                }
            }

            if (commandData.ChildrenCommandsId != null)
            {
                foreach (string childrenCommandId in commandData.ChildrenCommandsId)
                {
                    CliSharpCommandData? childrenCommandData =
                        commandsData.SingleOrDefault(x => x.Id == childrenCommandId);

                    if (childrenCommandData == null)
                        throw new InvalidOperationException(
                            InvalidCommandTheIdWasNotFound.Replace("$0", childrenCommandId));

                    VerifyParentRecursivity(command, childrenCommandData);

                    bool alreadyLoaded = commandsLoaded.ContainsKey(childrenCommandData.Id);
                    
                    ICliSharpCommand children = alreadyLoaded
                        ? commandsLoaded[childrenCommandData.Id]
                        : CliSharpCommand.Create(childrenCommandData.Id, childrenCommandData.Description);

                    command.AddCommand(children);

                    if (!alreadyLoaded)
                    {
                        LoadCommands(children, childrenCommandData);
                        commandsLoaded.Add(children.Id, children);
                    }
                }
            }
        }

        private static void VerifyParentRecursivity(ICliSharpCommand command, CliSharpCommandData childrenCommandData)
        {
            string tree = childrenCommandData.Id;
            ICliSharpCommand? commandCheck = command;

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

        private CliSharpData JsonSerializer(string path)
        {
            string config = GetDataFromFilePath(path);

            CliSharpData? data = JsonConvert.DeserializeObject<CliSharpData>(config);

            if (data == null)
                throw new ArgumentException(InvalidJson, nameof(path));

            return data;
        }

        private string GetDataFromFilePath(string path)
        {
            return fs.File.ReadAllText(path);
        }

        private CliSharpData YamlSerializer(string path)
        {
            string config = GetDataFromFilePath(path);

            var deserializer = new DeserializerBuilder().Build();

            return deserializer.Deserialize<CliSharpData>(config);
        }
    }
}