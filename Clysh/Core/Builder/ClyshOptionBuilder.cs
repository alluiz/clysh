using Clysh.Helper;

namespace Clysh.Core.Builder;

/// <summary>
/// A builder for <see cref="ClyshOption"/>
/// </summary>
/// <seealso cref="ClyshBuilder{T}"/>
public class ClyshOptionBuilder : ClyshBuilder<ClyshOption>
{
    

    private bool _hasProvidedOptionalParameterBefore;
    private int _lastParameterOrder = -1;

    /// <summary>
    /// The builder constructor
    /// </summary>
    public ClyshOptionBuilder()
    {
        
    }

    /// <summary>
    /// Build the option identifier
    /// </summary>
    /// <param name="id">The option identifier</param>
    /// <param name="shortcut">The option shortcut</param>
    /// <returns>An instance of <see cref="ClyshOptionBuilder"/></returns>
    public ClyshOptionBuilder Id(string id, string? shortcut = null)
    {
        try
        {
            Result.Id = id;
            Result.Shortcut = shortcut;

            ValidateShortcut(id, shortcut);

            return this;
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnCreateOption, id), e);
        }
    }

    private static void ValidateShortcut(string id, string? shortcut)
    {
        var reserved = new Dictionary<string, string>
        {
            { "help", "h" },
            { "version", "v" }
        };
        
        foreach (var pair in reserved)
        {
            if (!id.Equals(pair.Key) && pair.Value.Equals(shortcut))
                throw new ArgumentException(string.Format(ClyshMessages.ErrorOnValidateOptionShortcut, pair.Value, pair.Key, id), nameof(shortcut));
        }
    }

    /// <summary>
    /// Build the option description
    /// </summary>
    /// <param name="description">The option description</param>
    /// <returns>An instance of <see cref="ClyshOptionBuilder"/></returns>
    public ClyshOptionBuilder Description(string? description)
    {
        try
        {
            Result.Description = description;
            return this;
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnCreateOption, Result.Id), e);
        }
    }

    /// <summary>
    /// Build the option parameter
    /// </summary>
    /// <param name="parameter">The option parameter</param>
    /// <returns>An instance of <see cref="ClyshOptionBuilder"/></returns>
    public ClyshOptionBuilder Parameter(ClyshParameter parameter)
    {
        try
        {
            ValidateParameter(parameter);

            _hasProvidedOptionalParameterBefore = !parameter.Required;
            _lastParameterOrder = parameter.Order;
        
            Result.Parameters.Add(parameter);
            return this;
        }
        catch (Exception e)
        {
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnCreateOption, Result.Id), e);
        }
    }

    private void ValidateParameter(ClyshParameter parameterValue)
    {
        if (parameterValue.Order <= _lastParameterOrder)
            throw new ArgumentException(string.Format(ClyshMessages.ErrorOnValidateParameterOrder, parameterValue.Id), nameof(parameterValue));

        if (parameterValue.Required && _hasProvidedOptionalParameterBefore)
            throw new ArgumentException(string.Format(ClyshMessages.ErrorOnValidateParameterRequiredOrder, parameterValue.Id), nameof(parameterValue));
    }

    /// <summary>
    /// Build the option group
    /// </summary>
    /// <param name="group">The option group</param>
    /// <returns>An instance of <see cref="ClyshOptionBuilder"/></returns>
    public ClyshOptionBuilder Group(ClyshGroup group)
    {
        Result.Group = group;
        return this;
    }

    /// <summary>
    /// Build the option selected
    /// </summary>
    /// <param name="selected">The option selected</param>
    /// <returns>An instance of <see cref="ClyshOptionBuilder"/></returns>
    public ClyshOptionBuilder Selected(bool selected)
    {
        Result.Selected = selected;
        return this;
    }

    /// <summary>
    /// Reset the builder state
    /// </summary>
    protected override void Reset()
    {
        _lastParameterOrder = -1;
        _hasProvidedOptionalParameterBefore = false;
        base.Reset();
    }
}