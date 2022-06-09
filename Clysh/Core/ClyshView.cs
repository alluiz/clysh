using System;
using System.Linq;
using Clysh.Data;

namespace Clysh.Core;

public class ClyshView : IClyshView
{
    public ClyshData Data { get; set; }
    public int PrintedLines { get; private set; }

    public const string QuestionMustBeNotBlank = "Question must be not blank";

    private readonly IClyshConsole clyshConsole;
    private readonly bool printLineNumber;

    public ClyshView(
        IClyshConsole clyshConsole,
        ClyshData clyshData,
        bool printLineNumber = false)
    {
        this.clyshConsole = clyshConsole;
        Data = clyshData;
        this.printLineNumber = printLineNumber;
    }

    public string AskFor(string title)
    {
        return AskFor(title, false);
    }

    private string AskFor(string title, bool sensitive)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(QuestionMustBeNotBlank, nameof(title));

        Print($"{title}:", false, true);

        return sensitive ? clyshConsole.ReadSensitive() : clyshConsole.ReadLine();
    }

    public bool Confirm(string question = "Do you agree?", string yes = "Y", string no = "n")
    {
        return string.Equals(AskFor($"{question} ({yes}/{no})"), yes, StringComparison.CurrentCultureIgnoreCase);
    }

    public void PrintEmpty()
    {
        Print("");
    }

    public void Print(string? text)
    {
        Print(text, false, false);
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
    
    public void PrintWithoutBreak(string? text)
    {
        Print(text, false, true);
    }

    public string AskForSensitive(string title)
    {
        return AskFor(title, true);
    }

    private void PrintTitle()
    {
        PrintEmpty();
        Print($"{Data.Title}. Version: {Data.Version}");
        PrintEmpty();
    }

    public void PrintHelp(IClyshCommand command, Exception exception)
    {
        PrintException(exception);
        PrintHelp(command);
    }

    public void PrintHelp(IClyshCommand command)
    {
        PrintTitle();
        PrintCommand(command);
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

    public void PrintSeparator()
    {
        Print("-----------#-----------");
    }

    private void PrintCommand(IClyshCommand command)
    {
        var hasCommands = command.HasAnyChildren();

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