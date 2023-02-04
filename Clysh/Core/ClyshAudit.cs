using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// The audit for <see cref="Clysh"/>
/// </summary>
public class ClyshAudit
{
    private readonly IClyshIndexable obj;

    /// <summary>
    /// The Clysh error representation
    /// </summary>
    /// <param name="obj">The object with error</param>
    public ClyshAudit(IClyshIndexable obj)
    {
        this.obj = obj;
        Messages = new List<string>();
    }

    /// <summary>
    /// The object errors list
    /// </summary>
    public List<string> Messages { get; }

    /// <summary>
    /// Check if has any error
    /// </summary>
    /// <returns>The result</returns>
    public bool AnyError()
    {
        return Messages.Any();
    }

    /// <summary>
    /// Print audit result
    /// </summary>
    /// <returns>The audit text</returns>
    public override string ToString()
    {
        var line =  $"ObjectId: {obj.Id}, Type: {obj.GetType()}\n";

        line += $"    Message(s): [{Messages.Aggregate("\n        ", (current, message) => $"{current}{message}\n        ")}]";
        
        return line;
    }
}