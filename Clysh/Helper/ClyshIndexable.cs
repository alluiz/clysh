using System;
using System.Text.RegularExpressions;

namespace Clysh.Helper;

/// <summary>
/// The string indexable
/// </summary>
public abstract class ClyshIndexable: IClyshIndexable
{
    private string id = default!;
    private const string InvalidIdTheIdMustFollowThePatternId = "Invalid id. The id must follow the pattern: {0}. Id: '{1}'";

    /// <summary>
    /// The pattern to validate the id
    /// </summary>
    protected string? Pattern;

    private Regex? regex;

    /// <summary>
    /// The identifier
    /// </summary>
    public string Id
    {
        get => id;
        set => id = ValidatedId(value);
    }

    /// <summary>
    /// Validates the identifier
    /// </summary>
    /// <param name="identifier">The identifier</param>
    /// <returns>The validation result</returns>
    /// <exception cref="ArgumentException">The id must follow the pattern</exception>
    private string ValidatedId(string identifier)
    {
        if (Pattern == null) 
            return identifier;
        
        regex ??= new Regex(Pattern);

        if (!regex.IsMatch(identifier))
            throw new ArgumentException(
                string.Format(InvalidIdTheIdMustFollowThePatternId, Pattern, identifier),
                nameof(identifier));

        return identifier;
    }
}