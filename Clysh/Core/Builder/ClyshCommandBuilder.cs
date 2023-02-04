using Clysh.Helper;

namespace Clysh.Core.Builder;

/// <summary>
/// A builder for <see cref="ClyshCommand"/>
/// </summary>
/// <seealso cref="ClyshBuilder{T}"/>
public class ClyshCommandBuilder : ClyshBuilder<ClyshCommand>
{
    /// <summary>
    /// Set the command identifier
    /// </summary>
    /// <param name="id">The command identifier, eg: "level0.level1.level2"</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder Id(string id)
    {
        result.Id = id;
        result.Name = id.Split(".").Last();
        return this;
    }

    /// <summary>
    /// Set the command description
    /// </summary>
    /// <param name="description">The command description</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder Description(string? description)
    {
        ArgumentNullException.ThrowIfNull(description);

        try
        {
            result.Description = description;
            return this;
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnCreateCommand, result.Id), e);
        }
    }
    
    /// <summary>
    /// Set the command require subcommand flag
    /// </summary>
    /// <param name="require">The require flag</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder RequireSubcommand(bool require)
    {
        result.RequireSubcommand = require;
        return this;
    }

    /// <summary>
    /// Build the command option
    /// </summary>
    /// <param name="option">The command option</param>
    /// <param name="global">The global indicator</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder Option(ClyshOption option, bool global = false)
    {
        try
        {
            if (global)
                result.AddGlobalOption(option);
            else
                result.AddOption(option);
    
            return this;
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnCreateCommand, result.Id), e);
        }
    }

    /// <summary>
    /// Build the subcommand
    /// </summary>
    /// <param name="subCommand">The subcommand of the command</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder SubCommand(IClyshCommand subCommand)
    {
        try
        {
            result.AddSubCommand(subCommand);
            return this;
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnCreateCommand, result.Id), e);
        }
    }

    /// <summary>
    /// Build the group
    /// </summary>
    /// <param name="group">The group</param>
    /// <param name="global">The global indicator</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder Group(ClyshGroup group, bool global = false)
    {
        try
        {
            if (global)
                result.AddGlobalGroups(group);
            else
                result.AddGroups(group);
            
            return this;
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnCreateCommand, result.Id), e);
        }
    }
    
    /// <summary>
    /// Build the action
    /// </summary>
    /// <param name="action">The action of the command</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder Action(Action<IClyshCommand, IClyshView> action)
    {
        try
        {
            result.Action = action;
            return this;
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnCreateCommand, result.Id), e);
        }
    }
}