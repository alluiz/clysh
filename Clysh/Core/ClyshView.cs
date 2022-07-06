using System;
using System.Linq;
using Clysh.Data;

namespace Clysh.Core;

/// <summary>
/// The view of <see cref="Clysh"/>
/// </summary>
public class ClyshView : IClyshView
{
    private ClyshData Data { get; }
    
    /// <summary>
    /// The number of lines printed
    /// </summary>
    public int PrintedLines { get; private set; }

    /// <summary>
    /// Indicates if verbose mode is active
    /// </summary>
    public bool Verbose { get; set; }

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

    /// <summary>
    /// Ask for some data to user
    /// </summary>
    /// <param name="title">The title of the question</param>
    /// <returns></returns>
    public string AskFor(string title) => AskFor(title, false);

    /// <summary>
    /// Ask for some sensitive data to user
    /// </summary>
    /// <param name="title">The title of the question</param>
    /// <returns></returns>
    public string AskForSensitive(string title) => AskFor(title, true);

    /// <summary>
    /// Ask for user confirmation
    /// </summary>
    /// <param name="question">The question</param>
    /// <param name="yes">The value of positive answer</param>
    /// <param name="no">The value of negative answer</param>
    /// <returns></returns>
    public bool Confirm(string question = "Do you agree?", string yes = "Y", string no = "n")
    {
        return string.Equals(AskFor($"{question} ({yes}/{no})"), yes, StringComparison.CurrentCultureIgnoreCase);
    }

    /// <summary>
    /// Print text if verbose is active
    /// </summary>
    /// <param name="text">The text</param>
    public void PrintVerbose(string? text)
    {
       if (Verbose)
           Print(text);
    }

    /// <summary>
    /// Print text
    /// </summary>
    /// <param name="text">The text</param>
    public void Print(string? text) => Print(text, false, false);

    /// <summary>
    /// Print blank line
    /// </summary>
    public void PrintEmpty() => Print("");
    
    /// <summary>
    /// Prints help to user
    /// </summary>
    /// <param name="command">The command to print</param>
    /// <param name="exception">The exception</param>
    public void PrintHelp(IClyshCommand command, Exception exception)
    {
        PrintException(exception);
        PrintHelp(command);
    }

    /// <summary>
    /// Prints help to user
    /// </summary>
    /// <param name="command">The command to print</param>
    public void PrintHelp(IClyshCommand command)
    {
        PrintTitle();
        PrintCommand(command);
    }
    
    /// <summary>
    /// Prints a separator
    /// </summary>
    public void PrintSeparator(string separator = "#")
    {
        var length = separator.Length;
        var count = (45 - length) / 2;
        var symbol = string.Empty.PadRight(count, '-');
        
        Print($"{symbol}{separator}{symbol}");
    }

    /// <summary>
    /// Print text without line break
    /// </summary>
    /// <param name="text">The text</param>
    public void PrintWithoutBreak(string? text) => Print(text, false, true);

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
            PrintChildrenCommands(command);
        }
    }

    private void PrintChildrenCommands(IClyshCommand command)
    {
        Print("[commands]:");
        PrintEmpty();

        foreach (var item in command.SubCommands.OrderBy(obj => obj.Key)
                     .ToDictionary(obj => obj.Key, obj => obj.Value))
        {
            if (item.Key != command.Id)
            {
                Print("".PadRight(3) + $"{item.Key,-39}{item.Value.Description}");
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
              $"Parameters: (R)equired | (O)ptional = Length");
        PrintEmpty();

        foreach (var item in command.Options
                     .OrderBy(x => x.Value.Group?.Id)
                     .ThenBy(y=>y.Key))
        {
            var paramsText = item.Value.Parameters.ToString();

            Print("".PadRight(2) +
                  $"{(item.Value.Shortcut == null ? "" : "-" + item.Value.Shortcut),-10}--{item.Key,-13}{item.Value.Group?.Id,-15}{item.Value.Description,-55}{paramsText}");
        }

        PrintEmpty();
    }

    private void PrintHeader(IClyshCommand command, bool hasCommands)
    {
        var parent = command.Parent;
        var parentCommands = "";

        while (parent != null)
        {
            parentCommands = parent.Id + " " + parentCommands;
            parent = parent.Parent;
        }

        Print($"Usage: {parentCommands}{command.Id} [options]{(hasCommands ? " [commands]" : "")}");
        PrintEmpty();
        Print(command.Description);
        PrintEmpty();
    }
}