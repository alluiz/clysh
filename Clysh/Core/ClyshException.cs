namespace Clysh.Core;

public class ClyshException : InvalidOperationException
{
    public ClyshException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public ClyshException(string message):base(message)
    {
    }
}