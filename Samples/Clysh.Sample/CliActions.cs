using Clysh.Core;

namespace Clysh.Sample;

public static class CliActions
{
    public static void CalcOperationAdd(IClyshCommand cmd, IClyshView view)
    {
        CalcOperation(cmd, view, (a, b) => a + b);
        
        //Get data from parent
        var root = (Root) cmd.Parent!.Data["root"];

        view.Print($"={root.Message}");
    }
    
    public static void CalcOperationSub(IClyshCommand cmd, IClyshView view)
    {
        CalcOperation(cmd, view, (a, b) => a - b);
    }
    
    private static void CalcOperation(IClyshCommand cmd, IClyshView view, Func<int, int, int> operation)
    {
        var color = cmd.GetOptionFromGroup("color");
        var values = cmd.Options["values"];
        
        var a = GetValue(values.Selected? values.Parameters["a"].Data: view.AskFor("value (a)"));
        var b = GetValue(values.Selected? values.Parameters["b"].Data: view.AskFor("value (b)"));

        var result = operation(a, b);
        
        if (color != null)
        {
            if (color.Is("red"))
                view.Print($"={result}", ConsoleColor.Red);

            if (color.Is("blue"))
                view.Print($"={result}", ConsoleColor.Blue);
            
            if (color.Is("green"))
                view.Print($"={result}", ConsoleColor.Green);
        }
        else
        {
            var colorId = cmd.Options["color"].Parameters["COLOR_ID"].Data;
            
            switch (colorId)
            {
                case "RED":
                    view.Print($"={result}", ConsoleColor.Red);
                    break;
                case "BLUE":
                    view.Print($"={result}", ConsoleColor.Blue);
                    break;
                case "GREEN":
                    view.Print($"={result}", ConsoleColor.Green);
                    break;
                default:
                    view.Print($"={result}");
                    break;
            }
        }
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