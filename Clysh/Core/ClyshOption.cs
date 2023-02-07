using System.Text.RegularExpressions;
using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// The option for <see cref="Clysh"/>
/// </summary>
public class ClyshOption : ClyshEntity
{
    private const int MinShortcut = 1;
    private const int MaxShortcut = 1;

    private readonly Regex _regexShortcut;

    private readonly string _shorcutPattern;
    private string _description = string.Empty;

    /// <summary>
    /// The option constructor
    /// </summary>
    internal ClyshOption(): base(15, ClyshConstants.OptionPattern, 10, 150)
    {
        _shorcutPattern = @"[a-zA-Z]{1}";
        _regexShortcut = new Regex(_shorcutPattern);
        Parameters = new ClyshParameters();
    }

    /// <summary>
    /// The parameters
    /// </summary>
    public ClyshParameters Parameters { get; }

    /// <summary>
    /// The shortcut
    /// </summary>
    public string? Shortcut { get; internal set; }

    /// <summary>
    /// The status of option
    /// </summary>
    public bool Selected { get; internal set; }

    /// <summary>
    /// The group
    /// </summary>
    public ClyshGroup? Group { get; set; }

    /// <summary>
    /// The command owner
    /// </summary>
    public ClyshCommand? Command { get; internal set; }

    public bool IsGlobal { get; internal set; }

    /// <summary>
    /// Check the optionId
    /// </summary>
    /// <param name="id">The id to be checked</param>
    /// <returns>The result of validation</returns>
    public bool Is(string id)
    {
        return Id.Equals(id, StringComparison.CurrentCultureIgnoreCase);
    }

    private void ValidateShortcut()
    {
        if (Shortcut == null) 
            return;

        var reserved = new Dictionary<string, string>
        {
            { "help", "h" },
            { "version", "v" }
        };

        if (reserved.Any(pair => !Id.Equals(pair.Key) && pair.Value.Equals(Shortcut)))
        {
            throw new EntityException(string.Format(ClyshMessages.ErrorOnValidateOptionShortcut, Shortcut, Id));
        }
        
        if (IsValidShortcut(Shortcut))
            throw new EntityException(string.Format(ClyshMessages.ErrorOnValidateShorcut, _shorcutPattern, MinShortcut, MaxShortcut, Shortcut));
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

    internal override void Validate()
    {
        base.Validate();
        ValidateShortcut();
    }
}