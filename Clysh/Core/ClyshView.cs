using System.Diagnostics.CodeAnalysis;
using Clysh.Data;
using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// The view of <see cref="Clysh"/>
/// </summary>
public sealed class ClyshView : IClyshView
{
    private readonly IClyshConsole _clyshConsole;
    private readonly bool _printLineNumber;

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
        Data = clyshData;
        _clyshConsole = clyshConsole;
        _printLineNumber = printLineNumber;
    }

    /// <summary>
    /// The constructor of view
    /// </summary>
    /// <param name="clyshData">The data to print</param>
    /// <param name="printLineNumber">Indicates if should print the line number</param>
    [ExcludeFromCodeCoverage]
    public ClyshView(
        ClyshData clyshData,
        bool printLineNumber = false): this(new ClyshConsole(), clyshData, printLineNumber)
    {
        
    }

    private ClyshData Data { get; }

    public int PrintedLines { get; private set; }

    public bool Debug { get; set; }

    public string AskFor(string title) => AskFor(title, false);

    public string AskForSensitive(string title) => AskFor(title, true);

    public bool Confirm(string question = "Do you agree?", string yes = "Y", string no = "n")
    {
        return string.Equals(AskFor($"{question} ({yes}/{no})"), yes, StringComparison.CurrentCultureIgnoreCase);
    }

    public void PrintAlert(string text) => Print(text, ConsoleColor.DarkYellow);
    
    public void PrintDebug(string? text)
    {
       if (Debug)
           Print(text);
    }

    public void Print(string? text) => Print(text, false);

    public void Print(string? text, ConsoleColor foregroundColor, ConsoleColor backgroundColor = ConsoleColor.Black)
    {
        Console.ForegroundColor = foregroundColor;
        Console.BackgroundColor = backgroundColor;
        Print(text);
        Console.ResetColor();
    }

    public void PrintEmpty() => Print("");

    public void PrintError(string text) => Print(text, ConsoleColor.Red);

    public void PrintHelp(IClyshCommand command)
    {
        PrintVersion();
        PrintCommand(command);
    }

    public void PrintSeparator(string separator = "#")
    {
        var length = separator.Length;
        var count = (45 - length) / 2;
        var symbol = string.Empty.PadRight(count, '-');
        
        Print($"{symbol}{separator}{symbol}");
    }
    
    public void PrintSuccess(string text) => Print(text, ConsoleColor.Green);

    public void PrintWithoutBreak(string? text) => Print(text, true);

    private string AskFor(string title, bool sensitive)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(ClyshMessages.ErrorOnValidateUserInputQuestionAnswer, nameof(title));

        Print($"{title}:", true);

        return sensitive ? _clyshConsole.ReadSensitive() : _clyshConsole.ReadLine();
    }

    private void Print(string? text, bool noBreak)
    {
        PrintedLines++;

        if (_printLineNumber)
        {
            if (noBreak)
                _clyshConsole.Write(text, PrintedLines);
            else
                _clyshConsole.WriteLine(text, PrintedLines);
        }
        else
        {
            if (noBreak)
                _clyshConsole.Write(text);
            else
                _clyshConsole.WriteLine(text);
        }
    }

    public void PrintVersion()
    {
        PrintEmpty();
        Print($"{Data.Title}. Version: {Data.Version}");
        PrintEmpty();
    }

    public void PrintException(Exception exception)
    {
        PrintEmpty();
        PrintError($"Error: {exception.Message}");
        PrintEmpty();
    }

    private void PrintCommand(IClyshCommand command)
    {
        var hasCommands = command.AnySubcommand();

        PrintHeader(command, hasCommands);
        PrintOptions(command);

        if (hasCommands)
        {
            PrintSubCommands(command);
        }
    }

    private void PrintSubCommands(IClyshCommand command)
    {
        Print("[subcommands]:");
        PrintEmpty();

        foreach (var subCommand in command.SubCommands
                     .OrderBy(obj => obj.Key)
                     .Select(obj => obj.Value))
        {
            Print($"{string.Empty,-3}{subCommand.Name,-39}{subCommand.Description}");
        }

        PrintEmpty();
    }

    private void PrintOptions(IClyshCommand command)
    {
        Print("[options]:");
        PrintEmpty();
        Print($"{string.Empty,-3}{"Option",-22}{"Group",-11}{"Description",-35}{"Parameters", -29}");
        PrintEmpty();

        foreach (var option in command.Options
                     .OrderBy(x => x.Value.Group?.Id)
                     .ThenBy(y => y.Key)
                     .Select(z => z.Value))
        {
            PrintParameter(option);
        }

        PrintEmpty();
    }

    private void PrintParameter(ClyshOption option)
    {
        const int maxDescriptionlengthPerLine = 30;

        var description = option.Description;

        var truncate = description.Length > maxDescriptionlengthPerLine;

        var firstDescriptionLine = truncate ? description[..maxDescriptionlengthPerLine] : description;
        Print($"{string.Empty,-3}{option,-22}{option.Group?.Id,-11}{firstDescriptionLine,-35}{option.Parameters}");

        if (truncate)
            PrintDescriptionMultiline(maxDescriptionlengthPerLine, description);
    }

    private void PrintDescriptionMultiline(int maxDescriptionlengthPerLine, string description)
    {
        var startIndex = maxDescriptionlengthPerLine;

        var numberOfLines = description.Length / maxDescriptionlengthPerLine;

        for (var line = 1; line <= numberOfLines; line++)
        {
            Print(description[startIndex..].Length < maxDescriptionlengthPerLine
                ? $"{string.Empty,-36}{description[startIndex..]}"
                : $"{string.Empty,-36}{description.Substring(startIndex, maxDescriptionlengthPerLine)}");

            startIndex = maxDescriptionlengthPerLine * (line + 1);
        }
    }

    private void PrintHeader(IClyshCommand command, bool hasCommands)
    {
        var parentCommands = command.Id.Replace(".", " ");

        Print($"Usage: {parentCommands} [options]{(hasCommands ? " [subcommands]" : "")}");
        PrintEmpty();
        Print(command.Description);
        PrintEmpty();
    }
}