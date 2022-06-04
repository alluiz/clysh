namespace Clysh.Helper;

public abstract class ClyshBuilder<T>: IClyshBuilder<T> where T: new()
{
    protected T Result;

    protected ClyshBuilder()
    {
        Result = new T();
    }

    public void Reset()
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