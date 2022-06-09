using System;
using System.Text.RegularExpressions;
using Clysh.Helper;

namespace Clysh.Core;

public class ClyshParameter : ClyshSimpleIndexable
{
    private string? data;
    public string? Data { get { return data; } set { Validate(Id, value, MinLength, MaxLength); data = value; } }
    private readonly string? pattern;

    public Regex? Regex { get; set;  }
    public bool Required { get; set; }
    public int MinLength { get; set;  }
    public int MaxLength { get; set; }

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

    public ClyshParameter(string id, int minLength, int maxLength, bool required, string? pattern) : this(id, minLength, maxLength, required)
    {
        if (pattern != null)
        {
            this.pattern = pattern;
            Regex = new Regex(pattern);
        }
    }

    private void Validate(string field, string? value, int min, int max)
    {
        if (value == null || value.Trim().Length < min || value.Trim().Length > max)
            throw new ArgumentException($"Parameter {field} must be not null or empty and between {min} and {max} chars.", field);

        if (Regex != null && !Regex.IsMatch(value))
            throw new ArgumentException($"Parameter {field} must match the follow regex pattern: {pattern}.", field);

    }

    public override string ToString()
    {
        return Id + ":" + Data;
    }
}