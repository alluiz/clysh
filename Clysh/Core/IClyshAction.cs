namespace Clysh.Core;

public interface IClyshAction
{
    void Execute(IClyshCommand cmd, IClyshView view);
}