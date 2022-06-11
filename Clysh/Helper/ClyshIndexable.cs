namespace Clysh.Helper;

/// <summary>
/// The indexable <see cref="Clysh"/> class
/// </summary>
/// <typeparam name="T">The type to be indexable</typeparam>
public abstract class ClyshIndexable<T> : IClyshIndexable<T>
{
    private T id = default!;

    /// <summary>
    /// The identifier
    /// </summary>
    public T Id
    {
        get => id;
        set => id = ValidatedId(value);
    }

    /// <summary>
    /// Validate the identifier
    /// </summary>
    /// <param name="identifier">The identifier</param>
    /// <returns>The identifier</returns>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual T ValidatedId(T identifier)
    {
        if (identifier == null)
            throw new ArgumentNullException(nameof(identifier), "Invalid id.");
            
        return identifier;
    }
}