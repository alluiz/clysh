namespace Clysh.Helper;

public interface IClyshIndexable<out T>
{
    T Id { get; }
}