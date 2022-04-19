using CliSharp.Data;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace CliSharp
{
    public class CliSharpDataSetup
    {
        private readonly List<CliSharpCommandData> commandsData;

        public CliSharpDataSetup()
        {
            this.commandsData = new List<CliSharpCommandData>();
        }

        public ICliSharpCommand ParseCommands(string path)
        {
            CliSharpData? config;

            if (!File.Exists(path))
                throw new ArgumentException("Invalid path: CLI config file was not found.", nameof(path));

            if (Path.GetExtension(path).Equals(".json"))
                config = JsonSerializer(path);
            else if (Path.GetExtension(path).Equals(".yml"))
                config = YamlSerializer(path);
            else
                throw new ArgumentException("Invalid extension. Only JSON and YAML files are supported.", nameof(path));

            if (config == null)
                throw new ArgumentNullException(nameof(config), "Config must be not null.");

            return GetCommandsTree(config);
        }

        private ICliSharpCommand GetCommandsTree(CliSharpData sharpData)
        {
            try
            {
                if (sharpData.CommandsData == null)
                    throw new ArgumentNullException(nameof(sharpData), "Config must have at least one command and one root command.");

                if (sharpData.CommandsData.DistinctBy(x => x.Id).Count() != sharpData.CommandsData.Count)
                    throw new ArgumentException($"Invalid commandId. The id must be unique check your schema and try again.", nameof(sharpData));

                commandsData.AddRange(sharpData.CommandsData);

                CliSharpCommandData? rootConfig = sharpData.CommandsData.SingleOrDefault(x => x.Root);

                if (rootConfig == null)
                    throw new ArgumentNullException(nameof(sharpData), "Config must have at least one command and one root command.");

                ICliSharpCommand? root = GetCommandFromTree(rootConfig.Id);

                if (root == null)
                    throw new ArgumentNullException(nameof(sharpData), "Config must have at least one command and one root command.");

                return root;
            }
            catch (Exception e)
            {
                throw new ArgumentNullException("Invalid config setup.", e);
            }
        }


        private ICliSharpCommand? GetCommandFromTree(string? commandId)
        {
            ICliSharpCommand? command = null;

            if (commandId != null)
            {
                CliSharpCommandData? commandConfig = commandsData.SingleOrDefault(x => x.Id == commandId);

                if (commandConfig == null)
                    throw new ArgumentException($"Invalid commandId. The id was not found on CLICommands list.", nameof(commandId));

                command = CliSharpCommand.Create(commandConfig.Id, commandConfig.Description);

                if (commandConfig.OptionsData != null)
                {
                    foreach (CliSharpOptionData option in commandConfig.OptionsData)
                    {
                        if (option.ParametersData != null)
                        {
                            CliSharpParameters parameters = CliSharpParameters.Create(option.ParametersData.Select(x => new CliSharpParameter(x.Id, x.MinLength, x.MaxLength, x.Required, x.Pattern)).ToArray());
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
                        ICliSharpCommand? childrenCommand = GetCommandFromTree(childrenCommandId);
    
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