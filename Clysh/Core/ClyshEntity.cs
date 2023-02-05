using System.Text.RegularExpressions;
using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// The string indexable
/// </summary>
public abstract class ClyshEntity: IClyshEntity
{
    /// <summary>
    /// The ID max length
    /// </summary>
    private readonly int _maxIdLength;
    
    /// <summary>
    /// The description max length
    /// </summary>
    private readonly int _maxDescriptionLength;
    
    /// <summary>
    /// The description min length
    /// </summary>
    private readonly int _minDescriptionLength;
    
    /// <summary>
    /// The pattern to validate the ID. Could be null if no pattern is required.
    /// </summary>
    private readonly string? _pattern;

    private Regex? _regex;
    
    private readonly bool _requireDescription;

    protected ClyshEntity(int maxIdLength, int minDescriptionLength, int maxDescriptionLength = 0, string? pattern = null)
    {
        _maxIdLength = maxIdLength;
        _minDescriptionLength = minDescriptionLength;
        _maxDescriptionLength = maxDescriptionLength;
        _requireDescription = minDescriptionLength > 0;
        _pattern = pattern;
    }

    /// <summary>
    /// The ID text
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Validates the ID
    /// </summary>
    /// <param name="Id">The desired ID to be validated</param>
    /// <exception cref="ArgumentException">The ID is invalid.</exception>
    private void ValidateId()
    {
        if (Id == null)
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnValidateIdPattern, _pattern, Id));

        if (_maxIdLength > 0 && Id.Length > _maxIdLength)
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnValidateIdLength, _maxIdLength, Id));

        //No validation if no pattern was provided before.
        if (_pattern == null)
            return;
        
        _regex ??= new Regex(_pattern);

        if (!_regex.IsMatch(Id))
            throw new ClyshException(string.Format(ClyshMessages.ErrorOnValidateIdPattern, _pattern, Id));
    }
    
    private void ValidateDescription()
    {
        if (!_requireDescription)
            return;
        
        if (Description == null || Description.Trim().Length < _minDescriptionLength || Description.Trim().Length > _maxDescriptionLength)
            throw new ClyshException(
                string.Format(ClyshMessages.ErrorOnValidateDescription, _minDescriptionLength, _maxDescriptionLength, Description));
    }

    public virtual void Validate()
    {
        ValidateId();
        ValidateDescription();
    }
}