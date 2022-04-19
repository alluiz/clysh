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
        /// The CLI Commands list
        /// </summary>
        public ICliSharpCommand RootCommand { get; private set; }

        private readonly List<CliSharpCommandData> commandsData;

        /// <summary>
        /// The <b>CliSharpDataSetup</b> object
        /// </summary>
        /// <param name="pathOfData">The path of file. YAML or JSON format only</param>
        public CliSharpDataSetup(string pathOfData)
        {
            this.PathOfData = pathOfData;
            this.commandsData = new();
            LoadCommandsData();
        }

        private void LoadCommandsData()
        {
            CliSharpData? data;

            if (!File.Exists(PathOfData))
                throw new ArgumentException("Invalid path: CLI data file was not found.", nameof(PathOfData));

            if (Path.GetExtension(PathOfData).Equals(".json"))
                data = JsonSerializer(PathOfData);
            else if (Path.GetExtension(PathOfData).Equals(".yml"))
                data = YamlSerializer(PathOfData);
            else
                throw new ArgumentException("Invalid extension. Only JSON and YAML files are supported.",
                    nameof(PathOfData));

            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data must be not null.");

            LoadCommandsTree(data);
        }

        public void ParseCommands(string commandLineText, Action<CliSharpMap<CliSharpOption>, ICliSharpView> action)
        {
            ICliSharpCommand command = RootCommand;

            if (commandLineText != command.Id)
            {
                string[] commandsText = commandLineText.Split(" ");

                foreach (var commandId in commandsText)
                {
                    command = command.GetCommand(commandId);
                }
            }
            
            command.Action = action;
        }

        private void LoadCommandsTree(CliSharpData data)
        {
            try
            {
                if (data.CommandsData == null)
                    throw new ArgumentNullException(nameof(data),
                        "Config must have at least one command and one root command.");

                if (data.CommandsData.DistinctBy(x => x.Id).Count() != data.CommandsData.Count)
                    throw new ArgumentException(
                        $"Invalid commandId. The id must be unique check your schema and try again.", nameof(data));

                commandsData.AddRange(data.CommandsData);

                CliSharpCommandData? rootConfig = data.CommandsData.SingleOrDefault(x => x.Root);

                if (rootConfig == null)
                    throw new ArgumentNullException(nameof(data),
                        "Config must have at least one command and one root command.");

                RootCommand = LoadCommand(rootConfig.Id);
            }
            catch (Exception e)
            {
                throw new ArgumentNullException("Invalid config setup.", e);
            }
        }


        private ICliSharpCommand? LoadCommand(string? commandId)
        {
            ICliSharpCommand? command = null;

            if (commandId != null)
            {
                CliSharpCommandData? commandConfig = commandsData.SingleOrDefault(x => x.Id == commandId);

                if (commandConfig == null)
                    throw new ArgumentException($"Invalid commandId. The id was not found on CLICommands list.",
                        nameof(commandId));

                command = CliSharpCommand.Create(commandConfig.Id, commandConfig.Description);

                if (commandConfig.OptionsData != null)
                {
                    foreach (CliSharpOptionData option in commandConfig.OptionsData)
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

                if (commandConfig.ChildrenCommandsId != null)
                {
                    foreach (string childrenCommandId in commandConfig.ChildrenCommandsId)
                    {
                        ICliSharpCommand? childrenCommand = LoadCommand(childrenCommandId);

                        if (childrenCommand != null)
                            command.AddCommand(childrenCommand);
                    }
                }
            }

            return command;
        }

        private static CliSharpData? JsonSerializer(string path)
        {
            string config = GetConfigFromPath(path);

            return JsonConvert.DeserializeObject<CliSharpData>(config);
        }

        private static string GetConfigFromPath(string path)
        {
            string config;

            using (StreamReader sr = new(path))
            {
                config = sr.ReadToEnd();
            }

            return config;
        }

        private static CliSharpData YamlSerializer(string path)
        {
            string config = GetConfigFromPath(path);

            var deserializer = new DeserializerBuilder().Build();

            return deserializer.Deserialize<CliSharpData>(config);
        }
    }
}