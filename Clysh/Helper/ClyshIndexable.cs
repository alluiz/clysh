using System.Text.RegularExpressions;

namespace Clysh.Helper;

/// <summary>
/// The string indexable
/// </summary>
public abstract class ClyshIndexable: IClyshIndexable
{
    private string _id = default!;

    /// <summary>
    /// The pattern to validate the ID. Could be null if no pattern is required.
    /// </summary>
    protected string? pattern;
    
    /// <summary>
    /// The ID max length
    /// </summary>
    protected int maxLength = 0;
    
    private Regex? _regex;

    /// <summary>
    /// The ID text
    /// </summary>
    public string Id
    {
        get => _id;
        set => _id = ValidatedId(value);
    }

    /// <summary>
    /// Validates the ID
    /// </summary>
    /// <param name="desiredId">The desired ID to be validated</param>
    /// <returns>The validated ID</returns>
    /// <exception cref="ArgumentException">The ID is invalid.</exception>
    private string ValidatedId(string desiredId)
    {
        if (desiredId == null)
            throw new ArgumentException(string.Format(ClyshMessages.ErrorOnValidateIdPattern, pattern, desiredId), nameof(desiredId));

        if (maxLength > 0 && desiredId.Length > maxLength)
            throw new ArgumentException(string.Format(ClyshMessages.ErrorOnValidateIdLength, maxLength, desiredId), nameof(desiredId));

        //No validation if no pattern was provided before.
        if (pattern == null) 
            return desiredId;
        
        _regex ??= new Regex(pattern);

        if (!_regex.IsMatch(desiredId))
            throw new ArgumentException(string.Format(ClyshMessages.ErrorOnValidateIdPattern, pattern, desiredId), nameof(desiredId));

        return desiredId;
    }
}