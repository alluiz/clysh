using Clysh.Core;

namespace Clysh.Sample;

public class CalcRootAction: CalcAction
{
    public override void Execute(ClyshCommand cmd, IClyshView view)
    {
        cmd.Data.Add("root", new Root() { Message="Thanks for using my calc!" });
    }
}