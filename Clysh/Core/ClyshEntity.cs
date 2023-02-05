using System.Text.RegularExpressions;
using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// The string indexable
/// </summary>
public abstract class ClyshEntity: IClyshEntity
{
    /// <summary>
    /// The entity constructor
    /// </summary>
    /// <param name="idMaxLength">The maximum length of ID</param>
    /// <param name="idPattern">The ID pattern</param>
    /// <param name="descriptionMinLength">The minimum length of description. If is ZERO or less, description is not required</param>
    /// <param name="descriptionMaxLength">The maximum length of description.</param>
    /// <exception cref="ArgumentNullException">The ID maximum length is less OR equal than ZERO.</exception>
    /// <exception cref="ArgumentException">The ID maximum length is less OR equal than ZERO.</exception>
    /// <exception cref="ArgumentException">The description minimum length is greater than description maximum length.</exception>
    /// <exception cref="ArgumentException">The description maximum length is less than ZERO.</exception>
    protected ClyshEntity(int idMaxLength, string? idPattern = null, int descriptionMinLength = 0, int descriptionMaxLength = 0)
    {
        if (idMaxLength <= 0)
            throw new ArgumentException("The ID maximum length is less OR equal than ZERO.", nameof(descriptionMaxLength));
        
        _idMaxLength = idMaxLength;
        _idPattern = idPattern;

        if (descriptionMinLength > descriptionMaxLength)
            throw new ArgumentException("The description minimum length is greater than description maximum length.", nameof(descriptionMinLength));
        
        if (descriptionMaxLength < 0)
            throw new ArgumentException("The description maximum length is less than ZERO.", nameof(descriptionMaxLength));

        _descriptionMinLength = descriptionMinLength;
        _descriptionMaxLength = descriptionMaxLength;
        _requireDescription = descriptionMinLength > 0;
    }
    
    /// <summary>
    /// The ID max length
    /// </summary>
    private readonly int _idMaxLength;
    
    /// <summary>
    /// The description max length
    /// </summary>
    private readonly int _descriptionMaxLength;
    
    /// <summary>
    /// The description min length
    /// </summary>
    private readonly int _descriptionMinLength;
    
    /// <summary>
    /// The pattern to validate the ID. Could be null if no pattern is required.
    /// </summary>
    private readonly string? _idPattern;

    /// <summary>
    /// Indicates the entity requires description
    /// </summary>
    private readonly bool _requireDescription;

    /// <summary>
    /// The ID text
    /// </summary>
    public string Id { get; internal set; } = string.Empty;

    /// <summary>
    /// The description
    /// </summary>
    public string Description { get; internal set; } = string.Empty;

    /// <summary>
    /// Validates the ID
    /// </summary>
    /// <exception cref="ArgumentException">The ID is invalid.</exception>
    private void ValidateId()
    {
        if (_idMaxLength > 0 && Id.Length > _idMaxLength)
            throw new EntityException(string.Format(ClyshMessages.ErrorOnValidateIdLength, _idMaxLength, Id));

        //No validation if no pattern was provided before.
        if (_idPattern == null)
            return;
        
        var regex = new Regex(_idPattern);

        if (!regex.IsMatch(Id))
            throw new EntityException(string.Format(ClyshMessages.ErrorOnValidateIdPattern, _idPattern, Id));
    }
    
    private void ValidateDescription()
    {
        if (!_requireDescription)
            return;
        
        if (Description.Length < _descriptionMinLength || Description.Length > _descriptionMaxLength)
            throw new EntityException(string.Format(ClyshMessages.ErrorOnValidateDescription,
                _descriptionMinLength,
                _descriptionMaxLength,
                Description));
    }

    public virtual void Validate()
    {
        ValidateId();
        ValidateDescription();
    }
}