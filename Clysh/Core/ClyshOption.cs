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

    private const int MaxDescription = 150;
    private const int MinDescription = 10;

    private readonly Regex _regexShortcut;

    private readonly string _shorcutPattern;
    private string _description = string.Empty;

    private string? _shortcut;

    /// <summary>
    /// The option constructor
    /// </summary>
    public ClyshOption()
    {
        pattern = @"^[a-z]+(-{0,1}[a-z0-9]+)+$";
        maxLength = 15;
        _shorcutPattern = @"[a-zA-Z]{1}";
        _regexShortcut = new Regex(_shorcutPattern);
        Parameters = new ClyshParameters();
    }

    /// <summary>
    /// The description
    /// </summary>
    public string Description
    {
        get => _description; 
        set => _description = ValidateDescription(value);
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
        get => _shortcut;
        set => _shortcut = ValidateShortcut(value);
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

    public bool IsGlobal { get; set; }

    private static string ValidateDescription(string? descriptionValue)
    {
        if (descriptionValue == null || descriptionValue.Trim().Length is < MinDescription or > MaxDescription)
            throw new ArgumentException(
                string.Format(ClyshMessages.ErrorOnValidateDescription, MinDescription, MaxDescription, descriptionValue),
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
        if (IsValidShortcut(shortcutId))
            throw new ArgumentException(
                string.Format(ClyshMessages.ErrorOnValidateShorcut, _shorcutPattern, MinShortcut, MaxShortcut, shortcutId),
                nameof(shortcutId));

        return shortcutId;
    }

    private bool IsValidShortcut(string? shortcutId)
    {
        return shortcutId != null && (ShortcurtLength(shortcutId) || Format(shortcutId));
    }

    private bool Format(string shortcutId)
    {
        return !_regexShortcut.IsMatch(shortcutId);
    }

    private static bool ShortcurtLength(string shortcut)
    {
        return shortcut.Length is < MinShortcut or > MaxShortcut;
    }

    public override string ToString()
    {
        return $"{(Shortcut == null ? "" : "-" + Shortcut + ","),-4}--{Id}";
    }
}