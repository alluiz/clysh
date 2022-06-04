using System.Text.RegularExpressions;

namespace Clysh.Helper;

public abstract class ClyshSimpleIndexable: ClyshIndexable<string>
{
    private Regex? regex;
    protected string? Pattern;
    
    protected override string ValidatedId(string input)
    {
        input = base.ValidatedId(input);

        if (Pattern == null) 
            return input;
        
        regex ??= new Regex(Pattern);

        if (!regex.IsMatch(input))
            throw new ArgumentException($"Invalid id. The id must follow the pattern: {Pattern}", nameof(input));

        return input;
    }
}