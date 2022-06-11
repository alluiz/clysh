using System.Text.RegularExpressions;
using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// The parameter for <see cref="Clysh"/>
/// </summary>
public class ClyshParameter : ClyshSimpleIndexable
{
    private string? data;
    private readonly string? pattern;
    
    private Regex? Regex { get; set;  }
    
    /// <summary>
    /// The parameter data
    /// </summary>
    public string? Data { get { return data; } set { Validate(Id, value, MinLength, MaxLength); data = value; } }
    
    /// <summary>
    /// The indicator if parameter is required
    /// </summary>
    public bool Required { get; }
    
    /// <summary>
    /// The parameter data minimum length
    /// </summary>
    public int MinLength { get; }
    
    /// <summary>
    /// The parameter data maximum length
    /// </summary>
    public int MaxLength { get; }

    /// <summary>
    /// The parameter constructor
    /// </summary>
    /// <param name="id">The parameter identifier</param>
    /// <param name="minLength">The parameter data minimum length</param>
    /// <param name="maxLength">The parameter data maximum length</param>
    /// <param name="required">The indicator if parameter is required</param>
    /// <exception cref="ArgumentException">The length must be between 1 and 1000</exception>
    public ClyshParameter(string id, int minLength, int maxLength, bool required = true)
    {
        Id = id;
            
        if (minLength < 1)
            throw new ArgumentException($"Invalid min length. The values must be between 1 and 1000.", nameof(minLength));

        if (maxLength > 1000)
            throw new ArgumentException($"Invalid max length. The values must be between 1 and 1000.", nameof(maxLength));

        MinLength = minLength;
        MaxLength = maxLength;
        Required = required;
    }
    
    /// <summary>
    /// The parameter constructor
    /// </summary>
    /// <param name="id">The parameter identifier</param>
    /// <param name="minLength">The parameter data minimum length</param>
    /// <param name="maxLength">The parameter data maximum length</param>
    /// <param name="required">The indicator if parameter is required</param>
    /// <param name="pattern">The parameter data pattern validator</param>
    /// <exception cref="ArgumentException">The length must be between 1 and 1000</exception>
    public ClyshParameter(string id, int minLength, int maxLength, bool required, string? pattern) : this(id, minLength, maxLength, required)
    {
        if (pattern != null)
        {
            this.pattern = pattern;
            Regex = new Regex(pattern);
        }
    }

    /// <summary>
    /// Formats parameter exibition
    /// </summary>
    /// <returns>The parameter formatted</returns>
    public override string ToString()
    {
        return Id + ":" + Data;
    }
    
    private void Validate(string field, string? value, int min, int max)
    {
        if (value == null || value.Trim().Length < min || value.Trim().Length > max)
            throw new ArgumentException($"Parameter {field} must be not null or empty and between {min} and {max} chars.", field);

        if (Regex != null && !Regex.IsMatch(value))
            throw new ArgumentException($"Parameter {field} must match the follow regex pattern: {pattern}.", field);

    }
}