namespace Clysh.Core;

/// <summary>
/// The exception for <see cref="Clysh"/>
/// </summary>
public class ClyshException : InvalidOperationException
{
    /// <summary>
    /// The exception constructor
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="innerException">The inner exception</param>
    public ClyshException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// The exception constructor
    /// </summary>
    /// <param name="message">The message</param>
    public ClyshException(string message):base(message)
    {
    }
}