using System;
using System.Text.RegularExpressions;

namespace Clysh.Helper;

/// <summary>
/// The string indexable
/// </summary>
public abstract class ClyshIndexable: IClyshIndexable
{
    private Regex? regex;
    
    private string id = default!;

    /// <summary>
    /// The identifier
    /// </summary>
    public string Id
    {
        get => id;
        set => id = ValidatedId(value);
    }
    
    /// <summary>
    /// The pattern to validate the id
    /// </summary>
    protected string? Pattern;
    
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
            throw new ArgumentException($"Invalid id. The id must follow the pattern: {Pattern}", nameof(identifier));

        return identifier;
    }
}