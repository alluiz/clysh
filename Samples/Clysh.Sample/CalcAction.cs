using Clysh.Core;

namespace Clysh.Sample;

public abstract class CalcAction: IClyshActionV2
{
    protected static void CalcOperation(ICly cly, Func<int, int, int> operation)
    {
        var color = cly.Command.GetOptionFromGroup("color");
        var values = cly.Command.Options["values"];
        
        var a = GetValue(values.Selected? values.Parameters["a"].Data: cly.View.AskFor("value (a)"));
        var b = GetValue(values.Selected? values.Parameters["b"].Data: cly.View.AskFor("value (b)"));

        var result = operation(a, b);
        
        if (color != null)
        {
            if (color.Is("red"))
                cly.View.Print($"={result}", ConsoleColor.Red);

            if (color.Is("blue"))
                cly.View.Print($"={result}", ConsoleColor.Blue);
            
            if (color.Is("green"))
                cly.View.Print($"={result}", ConsoleColor.Green);
        }
        else
        {
            var colorId = cly.Command.Options["color"].Parameters["COLOR_ID"].Data;
            
            switch (colorId)
            {
                case "RED":
                    cly.View.Print($"={result}", ConsoleColor.Red);
                    break;
                case "BLUE":
                    cly.View.Print($"={result}", ConsoleColor.Blue);
                    break;
                case "GREEN":
                    cly.View.Print($"={result}", ConsoleColor.Green);
                    break;
                default:
                    cly.View.Print($"={result}");
                    break;
            }
        }
    }

    private static int GetValue(string value)
    {
        return Convert.ToInt32(value);
    }

    public abstract void Execute(ICly cly);
}