using System;
using System.Text.RegularExpressions;

namespace Clysh.Helper;

/// <summary>
/// The string indexable
/// </summary>
public abstract class ClyshIndexable: IClyshIndexable
{
    private string id = default!;
    private const string MessageInvalidId = "Invalid ID: The ID must follow the pattern: {0}. ID: '{1}'";

    /// <summary>
    /// The pattern to validate the ID. Could be null if no pattern is required.
    /// </summary>
    protected string? Pattern;

    private Regex? regex;

    /// <summary>
    /// The ID text
    /// </summary>
    public string Id
    {
        get => id;
        set => id = ValidatedId(value);
    }

    /// <summary>
    /// Validates the ID
    /// </summary>
    /// <param name="desiredId">The desired ID to be validated</param>
    /// <returns>The validated ID</returns>
    /// <exception cref="ArgumentException">The ID is invalid.</exception>
    private string ValidatedId(string desiredId)
    {
        //No validation if no pattern was provided before.
        if (Pattern == null) 
            return desiredId;
        
        regex ??= new Regex(Pattern);

        if (!regex.IsMatch(desiredId))
            throw new ArgumentException(string.Format(MessageInvalidId, Pattern, desiredId), nameof(desiredId));

        return desiredId;
    }
}