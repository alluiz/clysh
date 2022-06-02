using System.Runtime.Serialization;

namespace Clysh;

public class ClyshException : InvalidOperationException
{
    public ClyshException()
    {
    }

    protected ClyshException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public ClyshException(string? message) : base(message)
    {
    }

    public ClyshException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}