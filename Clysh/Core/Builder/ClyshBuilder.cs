namespace Clysh.Core.Builder;

/// <summary>
/// The builders base
/// </summary>
/// <typeparam name="T">The type to be builded</typeparam>
public abstract class ClyshBuilder<T> where T: new()
{
    /// <summary>
    /// The instance of type
    /// </summary>
    protected T Result;

    /// <summary>
    /// The builder constructor
    /// </summary>
    protected ClyshBuilder()
    {
        Result = new T();
    }

    /// <summary>
    /// Create a new instance of a type
    /// </summary>
    protected virtual void Reset()
    {
        Result = new T();
    }

    /// <summary>
    /// Finish build 
    /// </summary>
    /// <returns>The new instance</returns>
    public T Build()
    {
        var build = Result;
        Reset();
        return build;
    }
}