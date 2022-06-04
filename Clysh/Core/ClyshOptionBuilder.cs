using System.Text.RegularExpressions;
using Clysh.Helper;

namespace Clysh.Core;

public class ClyshOptionBuilder: ClyshBuilder<ClyshOption>
{
    private const int MaxDescription = 50;
    private const int MinDescription = 10;
        
    private const int MinShortcut = 1;
    private const int MaxShortcut = 1;
    
    private const string Pattern = "[a-zA-Z]";
    
    private readonly Regex regex;

    public ClyshOptionBuilder()
    {
        regex = new Regex(Pattern);
    }

    public ClyshOptionBuilder Id(string id)
    {
        Result.Id = id;
        return this;
    }

    public ClyshOptionBuilder Description(string description)
    {
        Validate(nameof(description), description, MinDescription, MaxDescription);
        Result.Description = description;
        return this;
    }
    
    public ClyshOptionBuilder Shortcut(string? shortcut)
    {
        if (shortcut != null && (shortcut.Length is < MinShortcut or > MaxShortcut || !regex.IsMatch(Pattern)))
            throw new ArgumentException($"Invalid shortcut. The shortcut must be null or follow the pattern {Pattern} and between {MinShortcut} and {MaxShortcut} chars.",
                nameof(shortcut));

        if (Result.Id is not "help" && shortcut is "h")
            throw new ArgumentException("Shortcut 'h' is reserved to help shortcut.", nameof(shortcut));
        
        Result.Shortcut = shortcut;
        return this;
    }
    
    public ClyshOptionBuilder Parameters(ClyshParameters parameters)
    {
        Result.Parameters = parameters;
        return this;
    }

    private static void Validate(string? field, string? value, int min, int max)
    {
        if (value == null || value.Trim().Length < min || value.Trim().Length > max)
            throw new ArgumentException($"Option {field} must be not null or empty and between {min} and {max} chars.", field);
    }
}