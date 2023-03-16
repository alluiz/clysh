namespace Clysh.Core;

public interface ICly
{
    IClyshView View { get; }
    IClyshCommand Command { get; }
}