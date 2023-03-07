using Clysh.Helper;

namespace Clysh.Core.Builder;

/// <summary>
/// A builder for <see cref="ClyshCommand"/>
/// </summary>
/// <seealso cref="ClyshBuilder{T}"/>
public sealed class ClyshCommandBuilder : ClyshBuilder<ClyshCommand>
{
    /// <summary>
    /// Set the command identifier
    /// </summary>
    /// <param name="id">The command identifier, eg: "level0.level1.level2"</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    /// <exception cref="ArgumentNullException">Thrown an exception if ID is null</exception>
    public ClyshCommandBuilder Id(string id)
    {
        result.Id = id ?? throw new ArgumentNullException(nameof(id));
        result.Name = id.Split(".").Last();
        return this;
    }

    /// <summary>
    /// Set the command description
    /// </summary>
    /// <param name="description">The command description</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>    ,
    /// <exception cref="ArgumentNullException">Thrown an exception if description is null</exception>
    public ClyshCommandBuilder Description(string description)
    {
        ArgumentNullException.ThrowIfNull(description);
        result.Description = description.Trim();
        return this;
    }

    /// <summary>
    /// Set the command require subcommand flag
    /// </summary>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder MarkAsAbstract()
    {
        result.Abstract = true;
        return this;
    }
    
    /// <summary>
    /// Set the command single flag
    /// </summary>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder IgnoreParents()
    {
        result.IgnoreParents = true;
        return this;
    }

    /// <summary>
    /// Build the command option
    /// </summary>
    /// <param name="option">The command option</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder Option(ClyshOption option)
    {
        result.AddOption(option);
        return this;
    }

    /// <summary>
    /// Build the subcommand
    /// </summary>
    /// <param name="subCommand">The subcommand of the command</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder SubCommand(ClyshCommand subCommand)
    {
        result.AddSubCommand(subCommand);
        return this;
    }

    /// <summary>
    /// Build the action
    /// </summary>
    /// <param name="action">The action of the command</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder Action(Action<IClyshCommand, IClyshView> action)
    {
        result.Action = action;
        return this;
    }

    /// <summary>
    /// Create a new instance of a type
    /// </summary>
    protected override void Reset()
    {
        result = new ClyshCommand();
    }

    /// <summary>
    /// Build the action
    /// </summary>
    /// <param name="action">The action of the command</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder Action(IClyshAction action)
    {
        return Action(action.Execute);
    }
}