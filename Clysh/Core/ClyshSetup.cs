using System.IO.Abstractions;
using System.Text.RegularExpressions;
using Clysh.Core.Builder;
using Clysh.Data;
using Clysh.Helper;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using FileSystem = System.IO.Abstractions.FileSystem;

namespace Clysh.Core;

/// <summary>
/// The setup of <see cref="Clysh"/>
/// </summary>
public class ClyshSetup : IClyshSetup
{
    /// <summary>
    /// The <b>ClyshDataSetup</b> object
    /// </summary>
    /// <param name="fs">The file system object</param>
    /// <param name="path">The path of file. YAML or JSON format only</param>
    public ClyshSetup(string path, IFileSystem fs)
    {
        _commandGlobalOptions = new Dictionary<string, List<ClyshOption>>();
        Commands = new ClyshMap<IClyshCommand>();
        Data = new ClyshData();
        Load(fs, path);
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
    /// The CLI Commands
    /// </summary>
    public ClyshMap<IClyshCommand> Commands { get; }

    private readonly Dictionary<string, List<ClyshOption>> _commandGlobalOptions;

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

    private void ExtractDataFromFileSystem(IFileSystem fs, string path)
    {
        if (!fs.File.Exists(path))
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnSetupLoadFilePath, path));

        var extension = fs.Path.GetExtension(path);

        Data = extension switch
        {
            ".json" => JsonSerializer(fs, path),
            ".yml" => YamlSerializer(fs, path),
            ".yaml" => YamlSerializer(fs, path),
            _ => throw new ClyshException(string.Format(ClyshMessages.ErrorOnSetupLoadFileExtension, path))
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
            throw new ClyshException(ClyshMessages.ErrorOnSetupCommands, e);
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
        var everyCommandHasParent = Commands.Count == Data.Commands!.Count;

        if (everyCommandHasParent) return;

        var commandsWithoutParent = new List<string>();
        Data.Commands.ForEach(c =>
        {
            if (!Commands.Has(c.Id))
                commandsWithoutParent.Add(c.Id);
        });

        throw new ClyshException(string.Format(ClyshMessages.ErrorOnSetupCommandsParent, string.Join(';', commandsWithoutParent)));
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
            throw new ClyshException(ClyshMessages.ErrorOnSetupCommandsDuplicatedRoot);
        }
    }

    private void VerifyCommands()
    {
        if (Data.Commands != null && Data.Commands.Any())
        {
            VerifyDuplicatedCommands(Data.Commands);
            VerifyCommandsPattern(Data.Commands);
        }
        else
            throw new ClyshException(ClyshMessages.ErrorOnSetupCommandsLength);
    }

    private static void VerifyCommandsPattern(List<CommandData> dataCommands)
    {
        const string pattern = ClyshConstants.CommandPattern;
        var regex = new Regex(pattern);

        dataCommands.ForEach(x=>
        {
            if (!regex.IsMatch(x.Id))
                throw new ClyshException(string.Format(ClyshMessages.ErrorOnValidateIdPattern, pattern, x.Id));
        });
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

        throw new ArgumentException(string.Format(ClyshMessages.ErrorOnSetupCommandsDuplicated, ids), nameof(commands));
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
            BuildCommandOptions(command, commandData);
            BuildCommandSubcommands(command);
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnCreateCommand, command.Id), e);
        }
    }

    private void BuildCommandSubcommands(IClyshCommand command)
    {
        var subcommands = GetSubcommands(command);

        if (!subcommands.Any())
        {
            if (command.RequireSubcommand)
                throw new ClyshException(string.Format(ClyshMessages.ErrorOnSetupSubCommands, command.Id));

            return;
        }

        foreach (var subcommandId in subcommands)
        {
            try
            {
                //Throw an error if command id was not found
                var subcommandData = Data.Commands!.SingleOrDefault(x => x.Id == subcommandId);

                if (subcommandData?.Id == null)
                    throw new ClyshException(string.Format(ClyshMessages.ErrorOnSetupCommandsNotFound, subcommandId));

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
                throw new ClyshException(string.Format(ClyshMessages.ErrorOnCreateSubCommand, subcommandId), e);
            }
        }
    }

    private List<string> GetSubcommands(IClyshCommand command)
    {
        var commandLevel = GetCommandLevel(command.Id);
        var nextLevel = commandLevel + 1;

        var subcommands = Data.Commands!.Where(c =>
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

    private void BuildCommandOptions(IClyshCommand command, CommandData commandData)
    {
        if (commandData.Options != null)
        {
            var groupBuilder = new ClyshGroupBuilder();
            var optionBuilder = new ClyshOptionBuilder();
            var groups = new ClyshMap<ClyshGroup>();
            
            foreach (var o in commandData.Options)
            {
                if (o.Group != null && !groups.Has(o.Group))
                    groups.Add(groupBuilder.Id(o.Group).Build());
                
                var option = BuildOption(optionBuilder, o, groups);
                
                if (option.Group != null)
                    command.AddGroups(option.Group);
                
                command.AddOption(option);
            }
        }

        if (!_commandGlobalOptions.ContainsKey(command.Id)) return;
        
        foreach (var option in _commandGlobalOptions[command.Id])
        {
            if (option.Group != null)
               command.AddGlobalGroups(option.Group);
    
            command.AddGlobalOption(option);
        }
    }

    private static ClyshOption BuildOption(ClyshOptionBuilder builder, OptionData option, ClyshMap<ClyshGroup> groups)
    {
        builder
            .Id(option.Id!, option.Shortcut)
            .Description(option.Description);

        BuildOptionGroup(builder, option, groups);
        BuildOptionParameters(builder, option);

        return builder.Build();
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

    private static void BuildOptionGroup(ClyshOptionBuilder builder, OptionData option, ClyshMap<ClyshGroup> groups)
    {
        if (option.Group == null) return;

        var group = groups[option.Group];

        group.Options.Add(option.Id!);

        builder.Group(group);
    }
    
    private ClyshData JsonSerializer(IFileSystem fs, string path)
    {
        var config = GetDataFromFilePath(fs, path);

        var data = JsonConvert.DeserializeObject<ClyshData>(config);

        if (data == null)
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnSetupLoadFileJson, path));

        return data;
    }

    private string GetDataFromFilePath(IFileSystem fs, string path)
    {
        return fs.File.ReadAllText(path);
    }

    private ClyshData YamlSerializer(IFileSystem fs, string path)
    {
        var data = GetDataFromFilePath(fs, path);

        var deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

        return deserializer.Deserialize<ClyshData>(data);
    }

    private void Load(IFileSystem fs, string path)
    {
        try
        {
            ExtractDataFromFileSystem(fs, path);
            CreateGlobalFromData();
            CreateCommandsFromData();
        }
        catch (ClyshException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnSetupLoad, path), e);
        }
    }

    private void CreateGlobalFromData()
    {
        var groupBuilder = new ClyshGroupBuilder();
        var optionBuilder = new ClyshOptionBuilder();
        var groups = new ClyshMap<ClyshGroup>();

        Data.GlobalOptions?.ForEach(o =>
        {
            if (o.Group != null && !groups.Has(o.Group))
                groups.Add(groupBuilder.Id(o.Group).Build());
            
            var option = BuildOption(optionBuilder, o, groups);

            foreach (var c in o.Commands!)
            {
                if (_commandGlobalOptions.ContainsKey(c))
                    _commandGlobalOptions[c].Add(option);
                else
                    _commandGlobalOptions.Add(c, new List<ClyshOption>
                    {
                        option
                    });
            }
        });
    }
}