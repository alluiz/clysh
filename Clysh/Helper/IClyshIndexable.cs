using Clysh.Core;

namespace Clysh.Helper;

public interface IClyshIndexable<T>
{
    T Id { get; set; }
}