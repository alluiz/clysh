using System;
using System.Text.RegularExpressions;

namespace Clysh.Core.Builder;

/// <summary>
/// A builder for <see cref="ClyshParameter"/>
/// </summary>
/// <seealso cref="ClyshBuilder{T}"/>
public class ClyshParameterBuilder: ClyshBuilder<ClyshParameter>
{
    /// <summary>
    /// Build the parameter identifier
    /// </summary>
    /// <param name="id">The parameter identifier</param>
    /// <returns>An instance of <see cref="ClyshParameterBuilder"/></returns>
    public ClyshParameterBuilder Id(string? id)
    {
        if (id == null)
            throw new ArgumentNullException(id);
        
        Result.Id = id;
        return this;
    }

    /// <summary>
    /// Build the parameter order
    /// </summary>
    /// <param name="order">The parameter order</param>
    /// <returns>An instance of <see cref="ClyshParameterBuilder"/></returns>
    public ClyshParameterBuilder Order(int order)
    {
        if (order < 0)
            throw new ClyshException("All parameters must be greater or equal than 0 order.");

        Result.Order = order;
        return this;
    }

    /// <summary>
    /// Build the parameter data pattern
    /// </summary>
    /// <param name="pattern">The parameter data pattern</param>
    /// <returns>An instance of <see cref="ClyshParameterBuilder"/></returns>
    public ClyshParameterBuilder Pattern(string? pattern)
    {
        if (pattern != null)
        {
            Result.PatternData = pattern;
            Result.Regex = new Regex(pattern);
        }

        return this;
    }

    /// <summary>
    /// Build the required status
    /// </summary>
    /// <param name="required">Indicates if the parameter is required for option</param>
    /// <returns>An instance of <see cref="ClyshParameterBuilder"/></returns>
    public ClyshParameterBuilder Required(bool required)
    {
        Result.Required = required;   
        return this;
    }

    /// <summary>
    /// Build the range of parameter data
    /// </summary>
    /// <param name="minLength">Indicates the minimum length</param>
    /// <param name="maxLength">Indicates the maximum length</param>
    /// <returns>An instance of <see cref="ClyshParameterBuilder"/></returns>
    public ClyshParameterBuilder Range(int minLength, int maxLength)
    {
        if (minLength < 1)
            throw new ArgumentException($"Invalid min length. The values must be between 1 and 1000.", nameof(minLength));
        
        if (maxLength > 1000)
            throw new ArgumentException($"Invalid max length. The values must be between 1 and 1000.", nameof(maxLength));
        
        Result.MinLength = minLength;
        Result.MaxLength = maxLength;
        
        if (Result.MinLength > Result.MaxLength)
            throw new ArgumentException($"Invalid max length. The max length must be greater than min length.", nameof(maxLength));
        
        return this;
    }
}