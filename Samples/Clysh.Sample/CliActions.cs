using Clysh.Core;
using Clysh.Helper;

namespace Clysh.Sample;

public static class CliActions
{
    public static void CalcOperationAdd(IClyshCommand cmd, IClyshView view)
    {
        CalcOperation(cmd.Options, view, (a, b) => a + b);
        
        //Get data from parent
        var root = (Root) cmd.Parent!.Data["root"];
        view.Print($"={root.Message}");
    }
    
    public static void CalcOperationSub(IClyshCommand cmd, IClyshView view)
    {
        CalcOperation(cmd.Options, view, (a, b) => a - b);
    }
    
    private static void CalcOperation(ClyshMap<ClyshOption> options, IClyshView view, Func<int, int, int> operation)
    {
        var values = options["values"];
        
        var a = GetValue(values.Selected? values.Parameters["a"].Data: view.AskFor("value (a)"));
        var b = GetValue(values.Selected? values.Parameters["b"].Data: view.AskFor("value (b)"));
        
        var result = operation(a, b);

        view.Print($"={result}");
    }

    private static int GetValue(string value)
    {
       return Convert.ToInt32(value);
    }

    public static void CalcRoot(IClyshCommand cmd, IClyshView view)
    {
        cmd.Data.Add("root", new Root() { Message="Thanks for using my calc!" });
    }
}