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
        Commands = new ClyshMap<ClyshCommand>();
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
    public ClyshCommand RootCommand { get; private set; } = default!;

    /// <summary>
    /// The CLI Data
    /// </summary>
    public ClyshData Data { get; private set; }

    /// <summary>
    /// The CLI Commands
    /// </summary>
    public ClyshMap<ClyshCommand> Commands { get; }

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
    
    /// <summary>
    /// Bind your command action
    /// </summary>
    /// <param name="commandId">The command id</param>
    /// <param name="action">The action that implements the IClyshAction interface</param>
    public void BindAction<T>(string commandId, T action) where T: IClyshAction
    {
        BindAction(commandId, action.Execute);
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

        var commandBuilder = GetCommandBuilder(rootData);

        RootCommand = BuildCommand(commandBuilder, rootData);

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
    /// <param name="commandBuilder">The command builder</param>
    /// <param name="commandData">The command data</param>
    /// <exception cref="ClyshException"></exception>
    private ClyshCommand BuildCommand(ClyshCommandBuilder commandBuilder, CommandData commandData)
    {
        try
        {
            BuildCommandOptions(commandBuilder, commandData);
            BuildCommandSubcommands(commandBuilder, commandData);

            var command = commandBuilder.Build();
            Commands.Add(commandData.Id, command);

            return command;
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnCreateCommand, commandData.Id), e);
        }
    }

    private void BuildCommandSubcommands(ClyshCommandBuilder commandBuilder, CommandData commandData)
    {
        var subcommands = GetSubcommands(commandData);
        
        foreach (var subcommandId in subcommands)
        {
            try
            {
                //Throw an error if command id was not found
                var subcommandData = Data.Commands!.SingleOrDefault(x => x.Id == subcommandId);

                if (subcommandData?.Id == null)
                    throw new ClyshException(string.Format(ClyshMessages.ErrorOnSetupCommandsNotFound, subcommandId));

                var subCommandBuilder = GetCommandBuilder(subcommandData);

                var subCommand = BuildCommand(subCommandBuilder, subcommandData);
                
                commandBuilder.SubCommand(subCommand);
            }
            catch (Exception e)
            {
                throw new ClyshException(string.Format(ClyshMessages.ErrorOnCreateSubCommand, subcommandId), e);
            }
        }
    }

    private static ClyshCommandBuilder GetCommandBuilder(CommandData commandData)
    {
        var subCommandBuilder = new ClyshCommandBuilder();

        subCommandBuilder
            .Id(commandData.Id)
            .Description(commandData.Description);
        
        if (commandData.Abstract)
            subCommandBuilder.MarkAsAbstract();
        
        if (commandData.IgnoreParents)
            subCommandBuilder.IgnoreParents();
        
        return subCommandBuilder;
    }

    private List<string> GetSubcommands(CommandData commandData)
    {
        var commandLevel = GetCommandLevel(commandData.Id);
        var nextLevel = commandLevel + 1;

        var subcommands = Data.Commands!.Where(c =>
                c.Id.Contains(commandData.Id) &&
                GetCommandLevel(c.Id) == nextLevel)
            .Select(c => c.Id)
            .ToList();

        return subcommands;
    }

    private static int GetCommandLevel(string commandId)
    {
        return commandId.Split(".", StringSplitOptions.RemoveEmptyEntries).Length - 1;
    }

    private void BuildCommandOptions(ClyshCommandBuilder commandBuilder, CommandData commandData)
    {
        if (commandData.Options != null)
        {
            var groupBuilder = new ClyshGroupBuilder();
            var optionBuilder = new ClyshOptionBuilder();
            var groups = new ClyshMap<ClyshGroup>();
            
            foreach (var optionData in commandData.Options)
            {
                if (optionData.Group != null && !groups.Has(optionData.Group))
                    groups.Add(groupBuilder
                        .Id(optionData.Group)
                        .Build());
                
                commandBuilder.Option(BuildOption(optionBuilder, optionData, groups));
            }
        }

        AddGlobalOptions(commandBuilder, commandData.Id);
        AddGlobalOptions(commandBuilder, "all");
    }

    private void AddGlobalOptions(ClyshCommandBuilder commandBuilder, string id)
    {
        if (!_commandGlobalOptions.ContainsKey(id)) return;
        
        foreach (var option in _commandGlobalOptions[id])
        {
            commandBuilder.Option(option);
        }
    }

    private static ClyshOption BuildOption(ClyshOptionBuilder builder,
        OptionData option,
        IReadOnlyDictionary<string, ClyshGroup> groups,
        bool globalOption = false)
    {
        builder
            .Id(option.Id, option.Shortcut)
            .Description(option.Description);

        if (globalOption)
            builder.MarkAsGlobal();
        
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
            if (p.Required)
                parameterBuilder.MarkAsRequired();
            
            builder.Parameter(parameterBuilder
                .Id(p.Id)
                .Order(p.Order)
                .Pattern(p.Pattern)
                .Range(p.MinLength, p.MaxLength)
                .Build());
        }
    }

    private static void BuildOptionGroup(ClyshOptionBuilder builder, OptionData optionData, IReadOnlyDictionary<string, ClyshGroup> groups)
    {
        if (optionData.Group == null) return;

        var group = groups[optionData.Group];

        group.Options.Add(optionData.Id);

        builder.Group(group);
    }
    
    private static ClyshData JsonSerializer(IFileSystem fs, string path)
    {
        var config = GetDataFromFilePath(fs, path);

        var data = JsonConvert.DeserializeObject<ClyshData>(config);

        if (data == null)
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnSetupLoadFileJson, path));

        return data;
    }

    private static string GetDataFromFilePath(IFileSystem fs, string path)
    {
        return fs.File.ReadAllText(path);
    }

    private static ClyshData YamlSerializer(IFileSystem fs, string path)
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
        if (Data.GlobalOptions == null) return;
        
        var groupBuilder = new ClyshGroupBuilder();
        var optionBuilder = new ClyshOptionBuilder();
        var groups = new ClyshMap<ClyshGroup>();

        foreach (var optionData in Data.GlobalOptions)
        {
            if (optionData.Group != null && !groups.Has(optionData.Group))
                groups.Add(groupBuilder
                    .Id(optionData.Group)
                    .Build());

            var option = BuildOption(optionBuilder, optionData, groups, true);

            optionData.Commands ??= new List<string>();

            var commands = optionData.Commands;
            
            if (commands.Count == 0) 
                commands.Add("all");

            if (commands.Contains("all") && commands.Count > 1)
                throw new ClyshException(string.Format(ClyshMessages.ErrorOnSetupGlobalOptionsAll, optionData.Id));
 
            foreach (var command in commands.Select(c => c.ToLower()))
            {
                if (_commandGlobalOptions.ContainsKey(command))
                    _commandGlobalOptions[command].Add(option);
                else
                    _commandGlobalOptions.Add(command, new List<ClyshOption>
                    {
                        option
                    });
            }
        }
    }
}