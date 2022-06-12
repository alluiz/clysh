using System;
using System.Text.RegularExpressions;

namespace Clysh.Helper;

/// <summary>
/// The string indexable
/// </summary>
public abstract class ClyshSimpleIndexable: ClyshIndexable<string>
{
    private Regex? regex;
    
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
    protected override string ValidatedId(string identifier)
    {
        identifier = base.ValidatedId(identifier);

        if (Pattern == null) 
            return identifier;
        
        regex ??= new Regex(Pattern);

        if (!regex.IsMatch(identifier))
            throw new ArgumentException($"Invalid id. The id must follow the pattern: {Pattern}", nameof(identifier));

        return identifier;
    }
}