using System;
using System.Text.RegularExpressions;
using Clysh.Helper;
using Microsoft.VisualBasic;

namespace Clysh.Core.Builder;

/// <summary>
/// A builder for <see cref="ClyshOption"/>
/// </summary>
/// <seealso cref="ClyshBuilder{T}"/>
public class ClyshOptionBuilder : ClyshBuilder<ClyshOption>
{
    

    private bool hasProvidedOptionalParameterBefore;
    private int lastParameterOrder = -1;

    /// <summary>
    /// The builder constructor
    /// </summary>
    public ClyshOptionBuilder()
    {
        
    }

    /// <summary>
    /// Build the option identifier
    /// </summary>
    /// <param name="id">The option identifier</param>
    /// <param name="shortcut">The option shortcut</param>
    /// <returns>An instance of <see cref="ClyshOptionBuilder"/></returns>
    public ClyshOptionBuilder Id(string id, string? shortcut = null)
    {
        try
        {
            Result.Id = id;
            Result.Shortcut = shortcut;

            ValidateHelpShortcut(id, shortcut);

            return this;
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnCreateOption, id), e);
        }
    }

    private static void ValidateHelpShortcut(string id, string? shortcut)
    {
        if (id is not "help" && shortcut is "h")
            throw new ArgumentException(string.Format(ClyshMessages.InvalidShortcutReserved, id), nameof(shortcut));
    }

    /// <summary>
    /// Build the option description
    /// </summary>
    /// <param name="description">The option description</param>
    /// <returns>An instance of <see cref="ClyshOptionBuilder"/></returns>
    public ClyshOptionBuilder Description(string? description)
    {
        try
        {
            Result.Description = description;
            return this;
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnCreateOption, Result.Id), e);
        }
    }

    /// <summary>
    /// Build the option parameter
    /// </summary>
    /// <param name="parameter">The option parameter</param>
    /// <returns>An instance of <see cref="ClyshOptionBuilder"/></returns>
    public ClyshOptionBuilder Parameter(ClyshParameter parameter)
    {
        try
        {
            ValidateParameter(parameter);

            hasProvidedOptionalParameterBefore = !parameter.Required;
            lastParameterOrder = parameter.Order;
        
            Result.Parameters.Add(parameter);
            return this;
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnCreateOption, Result.Id), e);
        }
    }

    private void ValidateParameter(ClyshParameter parameterValue)
    {
        if (parameterValue.Order <= lastParameterOrder)
            throw new ArgumentException(string.Format(ClyshMessages.InvalidParameterOrder, lastParameterOrder, parameterValue.Id), nameof(parameterValue));

        if (parameterValue.Required && hasProvidedOptionalParameterBefore)
            throw new ArgumentException(string.Format(ClyshMessages.InvalidParameterRequiredOrder, parameterValue.Id), nameof(parameterValue));
    }

    /// <summary>
    /// Build the option group
    /// </summary>
    /// <param name="group">The option group</param>
    /// <returns>An instance of <see cref="ClyshOptionBuilder"/></returns>
    public ClyshOptionBuilder Group(ClyshGroup group)
    {
        Result.Group = group;
        return this;
    }

    /// <summary>
    /// Build the option selected
    /// </summary>
    /// <param name="selected">The option selected</param>
    /// <returns>An instance of <see cref="ClyshOptionBuilder"/></returns>
    public ClyshOptionBuilder Selected(bool selected)
    {
        Result.Selected = selected;
        return this;
    }

    /// <summary>
    /// Reset the builder state
    /// </summary>
    protected override void Reset()
    {
        lastParameterOrder = -1;
        hasProvidedOptionalParameterBefore = false;
        base.Reset();
    }
}