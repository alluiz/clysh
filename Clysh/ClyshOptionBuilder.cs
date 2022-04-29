using System.Text.RegularExpressions;

namespace Clysh;

public class ClyshOptionBuilder
{
    private const int MaxDescription = 50;
    private const int MinDescription = 10;
        
    private const int MinShortcut = 1;
    private const int MaxShortcut = 1;
    
    private const string pattern = "[a-zA-Z]";
    
    private ClyshOption option;
    private readonly Regex regex;

    public ClyshOptionBuilder()
    {
        option = new ClyshOption();
        regex = new Regex(pattern);
    }

    public ClyshOptionBuilder Id(string id)
    {
        option.Id = id;
        return this;
    }

    public ClyshOptionBuilder Description(string description)
    {
        Validate(nameof(description), description, MinDescription, MaxDescription);
        option.Description = description;
        return this;
    }
    
    public ClyshOptionBuilder Shortcut(string shortcut)
    {
        if (shortcut.Length is < MinShortcut or > MaxShortcut || !regex.IsMatch(pattern))
            throw new ArgumentException($"Invalid shortcut. The shortcut must be null or follow the pattern {pattern} and between {MinShortcut} and {MaxShortcut} chars.",
                nameof(shortcut));

        if (this.option.Id is not "help" && shortcut is "h")
            throw new ArgumentException("Shortcut 'h' is reserved to help shortcut.", nameof(shortcut));
        
        option.Shortcut = shortcut;
        return this;
    }
    
    public ClyshOptionBuilder Parameters(ClyshParameters parameters)
    {
        option.Parameters = parameters;
        return this;
    }
    
    public ClyshOption Build()
    {
        ClyshOption build = this.option;
        this.option = new ClyshOption();
        return build;
    }
    
    private static void Validate(string? field, string? value, int min, int max)
    {
        if (value == null || value.Trim().Length < min || value.Trim().Length > max)
            throw new ArgumentException($"Option {field} must be not null or empty and between {min} and {max} chars.", field);
    }
}