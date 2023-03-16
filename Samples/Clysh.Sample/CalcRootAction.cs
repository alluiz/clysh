using Clysh.Core;

namespace Clysh.Sample;

public class CalcRootAction: CalcAction
{
    public override void Execute(ICly cly)
    {
        cly.View.Print("The root action is executed!");
        cly.Command.Data.Add("message", "Thanks for using my calc!");
    }
}