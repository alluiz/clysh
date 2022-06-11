namespace Clysh.Helper;

/// <summary>
/// The indexable interface for <see cref="Clysh"/>
/// </summary>
/// <typeparam name="T">The type to be indexable</typeparam>
public interface IClyshIndexable<out T>
{
    /// <summary>
    /// The identifier
    /// </summary>
    T Id { get; }
}