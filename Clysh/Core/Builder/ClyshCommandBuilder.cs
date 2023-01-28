using System;
using Clysh.Helper;

namespace Clysh.Core.Builder;

/// <summary>
/// A builder for <see cref="ClyshCommand"/>
/// </summary>
/// <seealso cref="ClyshBuilder{T}"/>
public class ClyshCommandBuilder : ClyshBuilder<ClyshCommand>
{
    private const string ErrorOnCreateCommand = "Error on create command. Command: {0}";

    /// <summary>
    /// Build the command identifier
    /// </summary>
    /// <param name="id">The command identifier, eg: "level0.level1.level2"</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder Id(string id)
    {
        Result.Id = id;
        Result.Name = id.Split(".").Last();
        return this;
    }

    /// <summary>
    /// Build the command description
    /// </summary>
    /// <param name="description">The command description</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder Description(string? description)
    {
        ArgumentNullException.ThrowIfNull(description);

        try
        {
            Result.Description = description;
            return this;
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ErrorOnCreateCommand, Result.Id), e);
        }
    }

    /// <summary>
    /// Build the command option
    /// </summary>
    /// <param name="option">The command option</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder Option(ClyshOption option)
    {
        try
        {
            Result.AddOption(option);
            return this;
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ErrorOnCreateCommand, Result.Id), e);
        }
    }

    /// <summary>
    /// Build the subcommand
    /// </summary>
    /// <param name="subCommand">The subcommand of the command</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder SubCommand(ClyshCommand subCommand)
    {
        try
        {
            Result.AddSubCommand(subCommand);
            return this;
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ErrorOnCreateCommand, Result.Id), e);
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
            Result.Action = action;
            return this;
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ErrorOnCreateCommand, Result.Id), e);
        }
    }

    /// <summary>
    /// Build the group
    /// </summary>
    /// <param name="group">The group</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder Group(ClyshGroup group)
    {
        try
        {
            Result.AddGroups(group);
            return this;
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ErrorOnCreateCommand, Result.Id), e);
        }
    }

    public ClyshCommandBuilder RequireSubcommand(bool require)
    {
        Result.RequireSubcommand = require;
        return this;
    }
}