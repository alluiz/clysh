using System;
using Clysh.Helper;

namespace Clysh.Core.Builder;

/// <summary>
/// A builder for <see cref="ClyshCommand"/>
/// </summary>
/// <seealso cref="ClyshBuilder{T}"/>
public class ClyshCommandBuilder: ClyshBuilder<ClyshCommand>
{
    private const int MaxDescription = 100;
    private const int MinDescription = 10;
    
    /// <summary>
    /// Build the command identifier
    /// </summary>
    /// <param name="id">The command identifier</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder Id(string? id)
    {
        if (id == null)
            throw new ArgumentNullException(id);
        
        Result.Id = id;
        return this;
    }
    
    /// <summary>
    /// Build the command description
    /// </summary>
    /// <param name="description">The command description</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder Description(string? description)
    {
        if (description == null || description.Trim().Length is < MinDescription or > MaxDescription)
            throw new ArgumentException($"Command {nameof(description)} value '{description}' must be not null or empty and between {MinDescription} and {MaxDescription} chars.", nameof(description));
        
        Result.Description = description;
        return this;
    }
    
    /// <summary>
    /// Build the command option
    /// </summary>
    /// <param name="option">The command option</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder Option(ClyshOption option)
    {
        Result.AddOption(option);
        return this;
    }

    /// <summary>
    /// Build the subcommand
    /// </summary>
    /// <param name="subCommand">The subcommand of the command</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder SubCommand(ClyshCommand subCommand)
    {
        Result.AddSubCommand(subCommand);
        return this;
    }

    /// <summary>
    /// Build the action
    /// </summary>
    /// <param name="action">The action of the command</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder Action(Action<IClyshCommand, ClyshMap<ClyshOption>, IClyshView> action)
    {
        Result.Action = action;
        return this;
    }

    /// <summary>
    /// Build the group
    /// </summary>
    /// <param name="group">The group</param>
    /// <returns>An instance of <see cref="ClyshCommandBuilder"/></returns>
    public ClyshCommandBuilder Group(ClyshGroup group)
    {
        Result.AddGroups(group);
        return this;
    }

    public ClyshCommandBuilder RequireSubcommand(bool require)
    {
        Result.RequireSubcommand = require;
        return this;
    }
}