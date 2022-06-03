using System.Runtime.Serialization;

namespace Clysh;

public class ClyshException : InvalidOperationException
{
    public ClyshException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}