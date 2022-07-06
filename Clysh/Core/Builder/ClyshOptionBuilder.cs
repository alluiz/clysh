using System;
using System.Text.RegularExpressions;

namespace Clysh.Core.Builder;

/// <summary>
/// A builder for <see cref="ClyshOption"/>
/// </summary>
/// <seealso cref="ClyshBuilder{T}"/>
public class ClyshOptionBuilder : ClyshBuilder<ClyshOption>
{
    private bool hasProvidedOptionalParameterBefore;
    private int lastParameterOrder = -1;
    
    private const int MaxDescription = 100;
    private const int MinDescription = 10;

    private const int MinShortcut = 1;
    private const int MaxShortcut = 1;

    private const string Pattern = "[a-zA-Z]";

    private readonly Regex regex;

    /// <summary>
    /// The builder constructor
    /// </summary>
    public ClyshOptionBuilder()
    {
        regex = new Regex(Pattern);
    }

    /// <summary>
    /// Build the option identifier
    /// </summary>
    /// <param name="id">The option identifier</param>
    /// <param name="shortcut">The option shortcut</param>
    /// <returns>An instance of <see cref="ClyshOptionBuilder"/></returns>
    public ClyshOptionBuilder Id(string? id, string? shortcut = null)
    {
        if (id == null)
            throw new ArgumentNullException(id);

        if (shortcut != null && (shortcut.Length is < MinShortcut or > MaxShortcut || !regex.IsMatch(Pattern)))
            throw new ArgumentException(
                $"Invalid shortcut. The shortcut must be null or follow the pattern {Pattern} and between {MinShortcut} and {MaxShortcut} chars.",
                nameof(shortcut));

        if (id is not "help" && shortcut is "h")
            throw new ArgumentException("Shortcut 'h' is reserved to help shortcut.", nameof(shortcut));

        Result.Id = id;
        Result.Shortcut = shortcut;
        return this;
    }

    /// <summary>
    /// Build the option description
    /// </summary>
    /// <param name="description">The option description</param>
    /// <returns>An instance of <see cref="ClyshOptionBuilder"/></returns>
    public ClyshOptionBuilder Description(string? description)
    {
        if (description == null || description.Trim().Length is < MinDescription or > MaxDescription)
            throw new ArgumentException(
                $"Option {nameof(description)} value '{description}' must be not null or empty and between {MinDescription} and {MaxDescription} chars.",
                nameof(description));

        Result.Description = description;
        return this;
    }

    /// <summary>
    /// Build the option parameter
    /// </summary>
    /// <param name="parameter">The option parameter</param>
    /// <returns>An instance of <see cref="ClyshOptionBuilder"/></returns>
    public ClyshOptionBuilder Parameter(ClyshParameter parameter)
    {
        if (parameter.Order <= lastParameterOrder)
            throw new ClyshException($"The order must be greater than the lastOrder: {lastParameterOrder}");

        if (parameter.Required && hasProvidedOptionalParameterBefore)
            throw new ClyshException(
                "Invalid order. The required parameters must come first than optional parameters. Check the order.");

        hasProvidedOptionalParameterBefore = !parameter.Required;
        lastParameterOrder = parameter.Order;
        
        Result.Parameters.Add(parameter);
        return this;
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