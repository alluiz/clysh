using Clysh.Core;

namespace Clysh.Sample;

public class CalcAddAction: CalcAction
{
    public override void Execute(ICly cly)
    {
        CalcOperation(cly, (a, b) => a + b);
                
        //Get data from parent
        var message = (string) cly.Command.Parent!.Data["message"];

        cly.View.Print($"={message}");
    }
}