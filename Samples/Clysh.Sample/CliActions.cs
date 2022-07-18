using Clysh.Core;
using Clysh.Helper;

namespace Clysh.Sample;

public static class CliActions
{
    public static void Calc(IClyshCommand cmd, ClyshMap<ClyshOption> options, IClyshView view)
    {
        var operation = cmd.GetOptionFromGroup("operation");

        if (operation == null)
            throw new InvalidOperationException("You must select some operation");

        var a = GetValue(operation.Parameters["a"].Data);
        var b = GetValue(operation.Parameters["b"].Data);
        var result = 0;
        
        if (operation.Is("add"))
            result = a + b;
        
        if (operation.Is("sub"))
            result = a - b;
        
        if (operation.Is("times"))
            result = a * b;

        if (operation.Is("by"))
            result = a / b;

        view.Print($"={result}");
    }

    private static int GetValue(string value)
    {
       return Convert.ToInt32(value);
    }
}