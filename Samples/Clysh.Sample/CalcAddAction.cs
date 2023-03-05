using Clysh.Core;

namespace Clysh.Sample;

public class CalcAddAction: CalcAction
{
    public override void Execute(ClyshCommand cmd, IClyshView view)
    {
        CalcOperation(cmd, view, (a, b) => a + b);
                
        //Get data from parent
        var root = (Root) cmd.Parent!.Data["root"];

        view.Print($"={root.Message}");
    }
}