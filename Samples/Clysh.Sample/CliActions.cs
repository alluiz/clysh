using Clysh.Core;
using Clysh.Helper;

namespace Clysh.Sample;

public static class CliActions
{
    public static void CalcOperationAdd(IClyshCommand cmd, ClyshMap<ClyshOption> options, IClyshView view)
    {
        CalcOperation(options, view, (a, b) => a + b);
    }
    
    public static void CalcOperationSub(IClyshCommand cmd, ClyshMap<ClyshOption> options, IClyshView view)
    {
        CalcOperation(options, view, (a, b) => a - b);
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
}