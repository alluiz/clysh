using System;
using System.Linq;
using Clysh.Data;

namespace Clysh.Core;

/// <summary>
/// The view of <see cref="Clysh"/>
/// </summary>
public class ClyshView : IClyshView
{
    private const string QuestionMustBeNotBlank = "Question must be not blank";

    private readonly IClyshConsole clyshConsole;
    private readonly bool printLineNumber;

    /// <summary>
    /// The constructor of view
    /// </summary>
    /// <param name="clyshConsole">The console to output</param>
    /// <param name="clyshData">The data to print</param>
    /// <param name="printLineNumber">Indicates if should print the line number</param>
    public ClyshView(
        IClyshConsole clyshConsole,
        ClyshData clyshData,
        bool printLineNumber = false)
    {
        this.clyshConsole = clyshConsole;
        Data = clyshData;
        this.printLineNumber = printLineNumber;
    }

    /// <summary>
    /// The constructor of view
    /// </summary>
    /// <param name="clyshData">The data to print</param>
    /// <param name="printLineNumber">Indicates if should print the line number</param>
    public ClyshView(
        ClyshData clyshData,
        bool printLineNumber = false): this(new ClyshConsole(), clyshData, printLineNumber)
    {
        
    }

    private ClyshData Data { get; }

    public int PrintedLines { get; private set; }

    public bool Debug { get; set; }

    public virtual string AskFor(string title) => AskFor(title, false);

    public virtual string AskForSensitive(string title) => AskFor(title, true);

    public virtual bool Confirm(string question = "Do you agree?", string yes = "Y", string no = "n")
    {
        return string.Equals(AskFor($"{question} ({yes}/{no})"), yes, StringComparison.CurrentCultureIgnoreCase);
    }

    public virtual void PrintDebug(string? text)
    {
       if (Debug)
           Print(text);
    }

    public virtual void Print(string? text) => Print(text, false, false);
    
    public virtual void Print(string? text, ConsoleColor foregroundColor, ConsoleColor backgroundColor = ConsoleColor.Black)
    {
        Console.ForegroundColor = foregroundColor;
        Console.BackgroundColor = backgroundColor;
        Print(text);
        Console.ResetColor();
    }

    public virtual void PrintEmpty() => Print("");
    
    public virtual void PrintHelp(IClyshCommand command, Exception exception)
    {
        PrintException(exception);
        PrintHelp(command);
    }

    public virtual void PrintHelp(IClyshCommand command)
    {
        PrintTitle();
        PrintCommand(command);
    }

    public virtual void PrintSeparator(string separator = "#")
    {
        var length = separator.Length;
        var count = (45 - length) / 2;
        var symbol = string.Empty.PadRight(count, '-');
        
        Print($"{symbol}{separator}{symbol}");
    }

    public virtual void PrintWithoutBreak(string? text) => Print(text, false, true);

    private string AskFor(string title, bool sensitive)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(QuestionMustBeNotBlank, nameof(title));

        Print($"{title}:", false, true);

        return sensitive ? clyshConsole.ReadSensitive() : clyshConsole.ReadLine();
    }

    private void Print(string? text, bool emptyLineAfterPrint, bool noBreak)
    {
        PrintedLines++;

        if (printLineNumber)
        {
            if (noBreak)
                clyshConsole.Write(text, PrintedLines);
            else
                clyshConsole.WriteLine(text, PrintedLines);
        }
        else
        {
            if (noBreak)
                clyshConsole.Write(text);
            else
                clyshConsole.WriteLine(text);
        }

        if (emptyLineAfterPrint)
            PrintEmpty();
    }

    private void PrintTitle()
    {
        PrintEmpty();
        Print($"{Data.Title}. Version: {Data.Version}");
        PrintEmpty();
    }

    private void PrintException(Exception exception)
    {
        PrintEmpty();
        PrintSeparator();
        PrintEmpty();
        Print($"Error: {exception.GetType().Name}: {exception.Message}");
        PrintEmpty();
        PrintSeparator();
    }

    private void PrintCommand(IClyshCommand command)
    {
        var hasCommands = command.HasAnySubcommand();

        PrintHeader(command, hasCommands);
        PrintOptions(command);

        if (hasCommands)
        {
            PrintSubCommands(command);
        }
    }

    private void PrintSubCommands(IClyshCommand command)
    {
        Print("[commands]:");
        PrintEmpty();

        foreach (var item in command.SubCommands.OrderBy(obj => obj.Key)
                     .ToDictionary(obj => obj.Key, obj => obj.Value))
        {
            if (item.Key != command.Id)
            {
                Print("".PadRight(3) + $"{item.Value.Name,-39}{item.Value.Description}");
            }
        }

        PrintEmpty();
    }

    private void PrintOptions(IClyshCommand command)
    {
        Print("[options]:");
        PrintEmpty();
        Print($"" +
              $"{"",-3}" +
              $"{"Shortcut",-11}" +
              $"{"Option",-13}" +
              $"{"Group",-15}" +
              $"{"Description",-55}" +
              $"Parameters: (R)equired | (O)ptional");
        PrintEmpty();

        foreach (var item in command.Options
                     .OrderBy(x => x.Value.Group?.Id)
                     .ThenBy(y => y.Key))
        {
            var paramsText = item.Value.Parameters.ToString();

            var desc = item.Value.Description!;
            var emptyLine = desc.Length > 50;
            
            if (emptyLine)
                PrintEmpty();
            
            PrintParameter(item, desc, paramsText);
        }

        PrintEmpty();
    }

    private void PrintParameter(KeyValuePair<string, ClyshOption> item, string desc, string paramsText)
    {
        Print(
            $"{"",-2}" +
              $"{(item.Value.Shortcut == null ? "" : "-" + item.Value.Shortcut),-10}" +
            $"--{item.Key,-13}" +
            $"{item.Value.Group?.Id,-15}" +
            $"{(desc.Length > 50 ? desc[..50] : desc),-55}" +
            $"{paramsText}");

        if (desc.Length <= 50) return;
        
        var startIndex = 50;

        for (var j = 1; j < desc.Length / 50; j++)
        {
            Print(desc[startIndex..].Length < 50
                ? $"{"",-42}{desc[startIndex..]}"
                : $"{"",-42}{desc.Substring(startIndex, 50)}");

            startIndex = 50 * (j + 1);
        }

        Print($"{"",-42}{desc[startIndex..]}");
    }

    private void PrintHeader(IClyshCommand command, bool hasCommands)
    {
        var parentCommands = command.Id.Replace(".", " ");

        Print($"Usage: {parentCommands} [options]{(hasCommands ? " [commands]" : "")}");
        PrintEmpty();
        Print(command.Description);
        PrintEmpty();
    }
}