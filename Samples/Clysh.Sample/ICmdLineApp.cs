using Clysh.Core;

namespace Clysh.Sample;

public interface ICmdLineApp
{
    public IClyshService Cli { get; }   
}