namespace Clysh.Helper;

public interface IClyshBuilder<out T>
{
    void Reset();
    T Build();
}