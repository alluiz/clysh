using Clysh.Core;

namespace Clysh.Sample;

public class CalcSubAction: CalcAction
{
    public override void Execute(ICly cly)
    {
        CalcOperation(cly, (a, b) => a - b);
    }
}