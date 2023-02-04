using System.Text.RegularExpressions;
using Clysh.Helper;

namespace Clysh.Core.Builder;

/// <summary>
/// A builder for <see cref="ClyshParameter"/>
/// </summary>
/// <seealso cref="ClyshBuilder{T}"/>
public class ClyshParameterBuilder : ClyshBuilder<ClyshParameter>
{
    /// <summary>
    /// Build the parameter identifier
    /// </summary>
    /// <param name="id">The parameter identifier</param>
    /// <returns>An instance of <see cref="ClyshParameterBuilder"/></returns>
    public ClyshParameterBuilder Id(string? id)
    {
        ArgumentNullException.ThrowIfNull(id);

        result.Id = id;
        return this;
    }

    /// <summary>
    /// Build the parameter order
    /// </summary>
    /// <param name="order">The parameter order</param>
    /// <returns>An instance of <see cref="ClyshParameterBuilder"/></returns>
    public ClyshParameterBuilder Order(int order)
    {
        try
        {
            result.Order = order;
            return this;
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnCreateParameter, result.Id), e);
        }
    }

    /// <summary>
    /// Build the parameter data pattern
    /// </summary>
    /// <param name="pattern">The parameter data pattern</param>
    /// <returns>An instance of <see cref="ClyshParameterBuilder"/></returns>
    public ClyshParameterBuilder Pattern(string? pattern)
    {
        try
        {
            if (pattern != null)
            {
                result.PatternData = pattern;
                result.Regex = new Regex(pattern);
            }
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnCreateParameter, result.Id), e);
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
        result.Required = required;
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
        try
        {
            if (minLength > maxLength)
                throw new ArgumentException(string.Format(ClyshMessages.ErrorOnValidateParameterMaxLength, result.Id), nameof(maxLength));
            
            result.MinLength = minLength;
            result.MaxLength = maxLength;   
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnCreateParameter, result.Id), e);
        }

        return this;
    }
}