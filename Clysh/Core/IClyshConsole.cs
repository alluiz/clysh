namespace Clysh.Core;

/// <summary>
/// Interface to implement your own console. Useful to logs, streams or something else.
/// </summary>
public interface IClyshConsole
{
    /// <summary>
    /// Read a line from console
    /// </summary>
    /// <returns>The input text in the console</returns>
    string ReadLine();
    
    /// <summary>
    /// Read a sensitive input from console
    /// </summary>
    /// <returns>The input text in the console</returns>
    string ReadSensitive();
    
    /// <summary>
    /// Write text with line break
    /// </summary>
    /// <param name="text">The text to be written to the console</param>
    void WriteLine(string? text);
    
    /// <summary>
    /// Write text with line break
    /// </summary>
    /// <param name="text">The text to be written to the console</param>
    /// <param name="lineNumber">The line number</param>
    void WriteLine(string? text, int lineNumber);

    /// <summary>
    /// Write text
    /// </summary>
    /// <param name="text">The text to be written to the console</param>
    void Write(string? text);
    
    /// <summary>
    /// Write text
    /// </summary>
    /// <param name="text">The text to be written to the console</param>
    /// <param name="lineNumber">The line number</param>
    void Write(string? text, int lineNumber);
}