using Clysh.Helper;

namespace Clysh.Core.Builder;

/// <summary>
/// The builders base
/// </summary>
/// <typeparam name="T">The type to be builded</typeparam>
public abstract class ClyshBuilder<T> where T: ClyshEntity
{
    protected ClyshBuilder()
    {
        Reset();
    }
    
    /// <summary>
    /// The instance of type
    /// </summary>
    protected T result = default!;

    protected abstract void Reset();

    /// <summary>
    /// Finish build 
    /// </summary>
    /// <returns>The new instance</returns>
    public T Build()
    {
        result.Validate();
        var build = result;
        Reset();
        return build;
    }
}