namespace Clysh.Helper;

/// <summary>
/// The indexable interface for <see cref="Clysh"/>
/// </summary>
/// <typeparam name="T">The type to be indexable</typeparam>
public interface IClyshIndexable
{
    /// <summary>
    /// The identifier
    /// </summary>
    string Id { get; }
}