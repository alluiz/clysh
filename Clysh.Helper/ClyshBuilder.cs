namespace Clysh.Helper;

public abstract class ClyshBuilder<T>: IClyshBuilder<T> where T: new()
{
    protected T Result;

    protected ClyshBuilder()
    {
        this.Result = new T();
    }

    public void Reset()
    {
        this.Result = new T();
    }

    public T Build()
    {
        var build = Result;
        this.Reset();
        return build;
    }
}