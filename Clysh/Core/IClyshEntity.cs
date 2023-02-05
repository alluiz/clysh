namespace Clysh.Core;

/// <summary>
/// The indexable interface for <see cref="Clysh"/>
/// </summary>
public interface IClyshEntity
{
    /// <summary>
    /// The identifier
    /// </summary>
    public string Id { get; }

    public void Validate();
}