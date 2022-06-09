using System;

namespace Clysh.Core;

/// <summary>
/// Interface to implement your own view. Useful to customize your interact with users
/// </summary>
/// <remarks>This is the CLI front-end</remarks>
/// <seealso cref="ClyshView"/>
public interface IClyshView
{
    /// <summary>
    /// The number of lines printed. Useful only for tests purpouses
    /// </summary>
    int PrintedLines { get; }
    
    /// <summary>
    /// A confirmation prompt.
    /// </summary>
    /// <param name="text">The question text. default: "Do you agree?"</param>
    /// <param name="yes">The text to positive answer. default: "Y"</param>
    /// <param name="no">The text to negative answer. default: "n"</param>
    /// <returns>The answer</returns>
    bool Confirm(string text = "Do you agree?", string yes = "Y", string no = "n");
    
    /// <summary>
    /// A input prompt.
    /// </summary>
    /// <param name="title">The title. Like: "Input your age"</param>
    /// <returns>The answer</returns>
    string AskFor(string title);
    
    /// <summary>
    /// A input prompt for sensitive data.
    /// </summary>
    /// <param name="title">The title. Like: "Input your password"</param>
    /// <returns>The sensitive answer</returns>
    string AskForSensitive(string title);
    
    /// <summary>
    /// Print empty line
    /// </summary>
    void PrintEmpty();
    
    /// <summary>
    /// Print text
    /// </summary>
    /// <param name="text">The text to be printed</param>
    void Print(string? text);

    /// <summary>
    /// Print text
    /// </summary>
    /// <param name="text">The text to be printed without line break</param>
    void PrintWithoutBreak(string? text);
    
    /// <summary>
    /// Print separator text
    /// </summary>
    void PrintSeparator();
    
    /// <summary>
    /// Print <see cref="IClyshCommand"/> help text
    /// </summary>
    /// <param name="command">The command to print help text</param>
    void PrintHelp(IClyshCommand command);

    /// <summary>
    /// Print <see cref="IClyshCommand"/> help text
    /// </summary>
    /// <param name="command">The command to print help text</param>
    /// <param name="exception">The exception to be printed with help</param>
    void PrintHelp(IClyshCommand command, Exception exception);
}