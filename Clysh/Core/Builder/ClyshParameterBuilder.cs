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
    /// <exception cref="ArgumentNullException">Thrown an error if ID is null</exception>
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
        result.Order = order;
        return this;
    }

    /// <summary>
    /// Build the parameter data pattern
    /// </summary>
    /// <param name="pattern">The parameter data pattern</param>
    /// <returns>An instance of <see cref="ClyshParameterBuilder"/></returns>
    public ClyshParameterBuilder Pattern(string? pattern)
    {
        if (pattern == null) return this;

        result.PatternData = pattern;
        result.Regex = new Regex(pattern);

        return this;
    }

    /// <summary>
    /// Build the required status
    /// </summary>
    /// <param name="required">Indicates if the parameter is required for option</param>
    /// <returns>An instance of <see cref="ClyshParameterBuilder"/></returns>
    public ClyshParameterBuilder MarkAsRequired()
    {
        result.Required = true;
        return this;
    }

    /// <summary>
    /// Build the range of parameter data
    /// </summary>
    /// <param name="minLength">Indicates the minimum length</param>
    /// <param name="maxLength">Indicates the maximum length</param>
    /// <returns>An instance of <see cref="ClyshParameterBuilder"/></returns>
    /// <exception cref="ArgumentException">Thrown an error if MIN length is greater than MAX length</exception>
    public ClyshParameterBuilder Range(int minLength, int maxLength)
    {
        if (minLength > maxLength)
            throw new ArgumentException(string.Format(ClyshMessages.ErrorOnValidateParameterMaxLength, result.Id),
                nameof(maxLength));

        result.MinLength = minLength;
        result.MaxLength = maxLength;

        return this;
    }

    /// <summary>
    /// Create a new instance of a type
    /// </summary>
    protected override void Reset()
    {
        result = new ClyshParameter();
    }
}