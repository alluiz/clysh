using System;
using System.Text.RegularExpressions;
using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// The parameter for <see cref="Clysh"/>
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class ClyshParameter : ClyshIndexable
{
    private const int minLengthParam = 1;
    private const int maxLengthParam = 1000;

    private string data;
    private int maxLength;
    private int minLength;
    private int order;

    /// <summary>
    /// Create a new parameter
    /// </summary>
    public ClyshParameter()
    {
        data = string.Empty;
    }

    /// <summary>
    /// The parameter data
    /// </summary>
    public string Data { 
        get => data;
        set { Validate(value); data = value; } }

    /// <summary>
    /// The parameter regex
    /// </summary>
    public Regex? Regex { get; set; }

    /// <summary>
    /// The parameter data regex pattern
    /// </summary>
    public string? PatternData { get; set; }

    /// <summary>
    /// The indicator if parameter is required
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// The parameter data minimum length
    /// </summary>
    public int MinLength
    {
        get => minLength; 
        set => minLength = ValidateMin(value);
    }

    /// <summary>
    /// The parameter data maximum length
    /// </summary>
    public int MaxLength
    {
        get => maxLength; 
        set => maxLength = ValidateMax(value);
    }

    /// <summary>
    /// Check if parameter data is filled
    /// </summary>
    public bool Filled { get; set; }

    /// <summary>
    /// Order of parameter
    /// </summary>
    public int Order
    {
        get => order; 
        set => order = ValidateOrder(value);
    }

    private int ValidateMin(int minLengthValue)
    {
        if (minLengthValue < 1)
            throw new ArgumentException(string.Format(ClyshMessages.ErrorOnValidateParameterRange, minLengthParam, maxLengthParam), nameof(minLengthValue));

        return minLengthValue;
    }

    private int ValidateMax(int maxValue)
    {
        if (maxValue > 1000)
            throw new ArgumentException(string.Format(ClyshMessages.ErrorOnValidateParameterRange, minLengthParam, maxLengthParam), nameof(maxValue));

        return maxValue;
    }

    private int ValidateOrder(int orderValue)
    {
        if (orderValue < 0)
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnValidateParameterOrder, Id));
        
        return orderValue;
    }

    /// <summary>
    /// Formats parameter exibition
    /// </summary>
    /// <returns>The parameter formatted</returns>
    public override string ToString()
    {
        return Id + ":" + Data;
    }

    private void Validate(string? value)
    {
        if (value == null || value.Trim().Length < MinLength || value.Trim().Length > MaxLength)
            throw new ArgumentException($"Parameter {Id} must be not null or empty and between {MinLength} and {MaxLength} chars.", Id);

        if (Regex != null && !Regex.IsMatch(value))
            throw new ArgumentException($"Parameter {Id} must match the follow regex pattern: {PatternData}.", Id);

    }
}