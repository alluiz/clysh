using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Clysh.Core;

/*
    This code is only for wrapper static System.Console class
    Therefore, it cannot be tested.
*/
[ExcludeFromCodeCoverage]
public class ClyshConsole : IClyshConsole
{
    /// <summary>
    /// Read a line from console
    /// </summary>
    /// <returns>The input text in the console or empty string</returns>
    public string ReadLine()
    {
        return Console.ReadLine() ?? "";
    }

    /// <summary>
    /// Read sensitive data from console without print at screen
    /// </summary>
    /// <remarks>
    /// It manipulate the cursor position if user press backspace.
    /// </remarks>
    /// <returns>
    /// The sensitive content
    /// </returns>
    public string ReadSensitive()
    {
        var data = new StringBuilder();

        var info = Console.ReadKey(true);

        while (info.Key != ConsoleKey.Enter)
        {
            if (info.Key != ConsoleKey.Backspace)
            {
                Console.Write("*");
                data.Append(info.KeyChar);
            }
            
            else if (info.Key == ConsoleKey.Backspace && !string.IsNullOrEmpty(data.ToString()))
            {
                // remove one character from the list of password characters
                data = data.Remove(data.Length - 1, 1);
                // get the location of the cursor
                var pos = Console.CursorLeft;
                // move the cursor to the left by one character
                Console.SetCursorPosition(pos - 1, Console.CursorTop);
                // replace it with space
                Console.Write(" ");
                // move the cursor to the left by one character again
                Console.SetCursorPosition(pos - 1, Console.CursorTop);
            }
            info = Console.ReadKey(true);
        }

        // add a new line because user pressed enter at the end of their password
        Console.WriteLine();

        return data.ToString();
    }

    /// <summary>
    /// Write text
    /// </summary>
    /// <param name="text">The text to be written to the console</param>
    public void Write(string? text)
    {
        Console.Write(text);
    }

    /// <summary>
    /// Write text
    /// </summary>
    /// <param name="text">The text to be written to the console</param>
    /// <param name="lineNumber">The line number</param>
    public void Write(string? text, int lineNumber)
    {
        Console.Write($"{lineNumber + ".",-5}{text}");
    }

    /// <summary>
    /// Write text with line break
    /// </summary>
    /// <param name="text">The text to be written to the console</param>
    public void WriteLine(string? text)
    {
        Console.WriteLine(text);
    }

    /// <summary>
    /// Write text with line break
    /// </summary>
    /// <param name="text">The text to be written to the console</param>
    /// <param name="lineNumber">The line number</param>
    public void WriteLine(string? text, int lineNumber)
    {
        Console.WriteLine($"{lineNumber + ".",-5}{text}");
    }
}