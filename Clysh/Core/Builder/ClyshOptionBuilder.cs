using System;
using System.Text.RegularExpressions;

namespace Clysh.Core.Builder;

/// <summary>
/// A builder for <see cref="ClyshOption"/>
/// </summary>
/// <seealso cref="ClyshBuilder{T}"/>
public class ClyshOptionBuilder: ClyshBuilder<ClyshOption>
{
    private const int MaxDescription = 50;
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
    public ClyshOptionBuilder Id(string id, string? shortcut = null)
    {
        if (shortcut != null && (shortcut.Length is < MinShortcut or > MaxShortcut || !regex.IsMatch(Pattern)))
            throw new ArgumentException($"Invalid shortcut. The shortcut must be null or follow the pattern {Pattern} and between {MinShortcut} and {MaxShortcut} chars.",
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
    public ClyshOptionBuilder Description(string description)
    {
        if (description == null || description.Trim().Length is < MinDescription or > MaxDescription)
            throw new ArgumentException($"Option {nameof(description)} value '{description}' must be not null or empty and between {MinDescription} and {MaxDescription} chars.", nameof(description));
        
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
}