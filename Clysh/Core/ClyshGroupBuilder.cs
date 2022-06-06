using Clysh.Helper;

namespace Clysh.Core;

public class ClyshGroupBuilder: ClyshBuilder<ClyshGroup>
{
    public ClyshGroupBuilder Id(string id)
    {
        Result.Id = id;
        return this;
    }
}