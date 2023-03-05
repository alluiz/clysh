using Clysh.Core;

namespace Clysh.Sample;

public class CalcRootAction: CalcAction
{
    public override void Execute(ClyshCommand cmd, IClyshView view)
    {
        view.Print("The root action is executed!");
        cmd.Data.Add("message", "Thanks for using my calc!");
    }
}