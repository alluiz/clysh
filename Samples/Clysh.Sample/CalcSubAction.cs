using Clysh.Core;

namespace Clysh.Sample;

public class CalcSubAction: CalcAction
{
    public override void Execute(IClyshCommand cmd, IClyshView view)
    {
        CalcOperation(cmd, view, (a, b) => a - b);
    }
}