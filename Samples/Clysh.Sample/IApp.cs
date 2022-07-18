using Clysh.Core;

namespace Clysh.Sample;

public interface IApp
{
    public IClyshService Cli { get; }
}