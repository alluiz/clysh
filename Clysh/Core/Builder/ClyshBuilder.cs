namespace Clysh.Core.Builder;

public abstract class ClyshBuilder<T> where T: new()
{
    protected T Result;

    protected ClyshBuilder()
    {
        Result = new T();
    }

    private void Reset()
    {
        Result = new T();
    }

    public T Build()
    {
        var build = Result;
        Reset();
        return build;
    }
}