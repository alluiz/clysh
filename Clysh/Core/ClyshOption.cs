using System;
using System.Text.RegularExpressions;
using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// The option for <see cref="Clysh"/>
/// </summary>
public class ClyshOption : ClyshIndexable
{
    private const int MinShortcut = 1;
    private const int MaxShortcut = 1;

    private const int MaxDescription = 500;
    private const int MinDescription = 10;

    private const string InvalidShorcutMessage = "Invalid shortcut. The shortcut must be null or follow the pattern {0} and between {1} and {2} chars. Shortcut: '{3}'";

    private const string InvalidDescription =
        "Option description must be not null or empty and between {0} and {1} chars. Description: '{2}'";

    private readonly Regex regexShortcut;

    private readonly string shorcutPattern;
    private string description = string.Empty;

    private string? shortcut;

    /// <summary>
    /// The option constructor
    /// </summary>
    public ClyshOption()
    {
        Pattern = @"^[a-z]+(-{0,1}[a-z0-9]+)+$";
        MaxLength = 30;
        shorcutPattern = @"[a-zA-Z]{1}";
        regexShortcut = new Regex(shorcutPattern);
        Parameters = new ClyshParameters();
    }

    /// <summary>
    /// The description
    /// </summary>
    public string? Description
    {
        get => description; 
        set => description = ValidateDescription(value);
    }

    /// <summary>
    /// The parameters
    /// </summary>
    public ClyshParameters Parameters { get; set; }

    /// <summary>
    /// The shortcut
    /// </summary>
    public string? Shortcut
    {
        get => shortcut;
        set => shortcut = ValidateShortcut(value);
    }

    /// <summary>
    /// The status of option
    /// </summary>
    public bool Selected { get; set; }

    /// <summary>
    /// The group
    /// </summary>
    public ClyshGroup? Group { get; set; }

    /// <summary>
    /// The command owner
    /// </summary>
    public IClyshCommand? Command { get; set; }

    private static string ValidateDescription(string? descriptionValue)
    {
        if (descriptionValue == null || descriptionValue.Trim().Length is < MinDescription or > MaxDescription)
            throw new ArgumentException(
                string.Format(InvalidDescription, MinDescription, MaxDescription, descriptionValue),
                nameof(descriptionValue));

        return descriptionValue;
    }

    /// <summary>
    /// Check the optionId
    /// </summary>
    /// <param name="id">The id to be checked</param>
    /// <returns>The result of validation</returns>
    public bool Is(string id)
    {
        return Id.Equals(id, StringComparison.CurrentCultureIgnoreCase);
    }

    private string? ValidateShortcut(string? shortcutId)
    {
        if (InvalidShortcut(shortcutId))
            throw new ArgumentException(
                string.Format(InvalidShorcutMessage, shorcutPattern, MinShortcut, MaxShortcut, shortcutId),
                nameof(shortcutId));

        return shortcutId;
    }

    private bool InvalidShortcut(string? shortcutId)
    {
        return shortcutId != null && (InvalidShortcurtLength(shortcutId) || InvalidFormat(shortcutId));
    }

    private bool InvalidFormat(string shortcutId)
    {
        return !regexShortcut.IsMatch(shortcutId);
    }

    private static bool InvalidShortcurtLength(string shortcut)
    {
        return shortcut.Length is < MinShortcut or > MaxShortcut;
    }
}