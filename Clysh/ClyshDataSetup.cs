using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Clysh.Data;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Clysh
{
    public class ClyshDataSetup
    {
        /// <summary>
        /// The path of file. YAML or JSON format only
        /// </summary>
        public string PathOfData { get; set; }

        /// <summary>
        /// The CLI Root command
        /// </summary>
        public IClyshCommand RootCommand { get; private set; }

        public ClyshData Data { get; set; }

        public const string InvalidPath = "Invalid path: CLI data file was not found.";
        public const string InvalidExtension = "Invalid extension. Only JSON (.json) and YAML (.yml or .yaml) files are supported.";
        public const string ErrorOnLoad = "Error on load data from file path.";
        public const string InvalidJson = "Invalid JSON: The deserialization results in null object.";
        public const string ErrorOnCreateRoot = "Error on create root from extracted data.";
        public const string InvalidCommandsTheIdSMustBeUnique = "Invalid commands: The id(s): $0 must be unique check your schema and try again.";
        public const string InvalidCommandsLength = $"Invalid commands: The data must contains at once one command.";
        public const string InvalidCommandTheIdWasNotFound = $"Invalid commandId. The id: $0 was not found on commands data list.";

        private readonly Dictionary<string, IClyshCommand> commandsLoaded;
        private readonly List<ClyshCommandData> commandsData;
        private readonly IFileSystem fs;

        /// <summary>
        /// The <b>ClyshDataSetup</b> object
        /// </summary>
        /// <param name="fs">The file system object</param>
        /// <param name="pathOfData">The path of file. YAML or JSON format only</param>
        public ClyshDataSetup(IFileSystem fs, string pathOfData)
        {
            this.fs = fs;
            PathOfData = pathOfData;
            commandsData = new();
            commandsLoaded = new();
            Data = new ClyshData();
            RootCommand = GetRootCommandFromFilePath(PathOfData);
        }

        /// <summary>
        /// The <b>ClyshDataSetup</b> object
        /// </summary>
        /// <param name="pathOfData">The path of file. YAML or JSON format only</param>
        public ClyshDataSetup(string pathOfData) : this(new FileSystem(), pathOfData)
        {
        }


        private IClyshCommand GetRootCommandFromFilePath(string path)
        {
            try
            {
                ExtractDataFromFile(path);

                return CreateRootFromExtractedData();
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
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

        public void MakeAction(string commandId, Action<ClyshMap<ClyshOption>, IClyshView> action)
        {
            IClyshCommand command = commandsLoaded[commandId];
            
            command.Action = action;
        }

        private IClyshCommand CreateRootFromExtractedData()
        {
            try
            {
                if (Data.Commands.DistinctBy(x => x.Id).Count() != Data.Commands.Count)
                    HandleIdsError(Data.Commands);
                else if (Data.Commands.Count == 0)
                    throw new ArgumentException(InvalidCommandsLength, nameof(Data));

                ClyshCommandData? rootData = Data.Commands.SingleOrDefault(x => x.Root);

                if (rootData == null)
                    throw new ArgumentNullException(nameof(Data), "Data must have at least one root command.");

                commandsData.AddRange(Data.Commands);

                IClyshCommand root = ClyshCommand.Create(rootData.Id, rootData.Description);
                commandsLoaded.Add(root.Id, root);

                LoadCommands(root, rootData);

                return root;
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(ErrorOnCreateRoot, e);
            }
        }

        private static void HandleIdsError(List<ClyshCommandData> commands)
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

        private void LoadCommands(IClyshCommand command, ClyshCommandData commandData)
        {
            if (commandData.Options != null)
            {
                foreach (ClyshOptionData option in commandData.Options)
                {
                    if (option.ParametersData != null)
                    {
                        ClyshParameters parameters = ClyshParameters.Create(option.ParametersData.Select(x =>
                                new ClyshParameter(x.Id, x.MinLength, x.MaxLength, x.Required, x.Pattern))
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
                    ClyshCommandData? childrenCommandData =
                        commandsData.SingleOrDefault(x => x.Id == childrenCommandId);

                    if (childrenCommandData == null)
                        throw new InvalidOperationException(
                            InvalidCommandTheIdWasNotFound.Replace("$0", childrenCommandId));

                    VerifyParentRecursivity(command, childrenCommandData);

                    bool alreadyLoaded = commandsLoaded.ContainsKey(childrenCommandData.Id);
                    
                    IClyshCommand children = alreadyLoaded
                        ? commandsLoaded[childrenCommandData.Id]
                        : ClyshCommand.Create(childrenCommandData.Id, childrenCommandData.Description);

                    command.AddCommand(children);

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
            string tree = childrenCommandData.Id;
            IClyshCommand? commandCheck = command;

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
            string config = GetDataFromFilePath(path);

            ClyshData? data = JsonConvert.DeserializeObject<ClyshData>(config);

            if (data == null)
                throw new ArgumentException(InvalidJson, nameof(path));

            return data;
        }

        private string GetDataFromFilePath(string path)
        {
            return fs.File.ReadAllText(path);
        }

        private ClyshData YamlSerializer(string path)
        {
            string config = GetDataFromFilePath(path);

            var deserializer = new DeserializerBuilder().Build();

            return deserializer.Deserialize<ClyshData>(config);
        }
    }
}