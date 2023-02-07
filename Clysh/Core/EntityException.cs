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
    public EntityException(string message):base(message)
    {
    }
}