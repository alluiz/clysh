using System.Runtime.Serialization;

namespace Clysh.Core;

/// <summary>
/// The exception for <see cref="ClyshEntity"/>
/// </summary>
[Serializable]
public class EntityException : InvalidOperationException
{
    /// <summary>
    /// The exception constructor
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="innerException">The inner exception</param>
    public EntityException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// The exception constructor
    /// </summary>
    /// <param name="message">The message</param>
    public EntityException(string message):base(message)
    {
    }

    /// <summary>
    /// Serialization constructor
    /// </summary>
    /// <param name="info">The serialization info</param>
    /// <param name="context">The streaming context</param>
    protected EntityException(SerializationInfo info,
        StreamingContext context): base(info, context)
    {
    }
}