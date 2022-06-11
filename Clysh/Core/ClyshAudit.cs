using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// The audit for <see cref="Clysh"/>
/// </summary>
public class ClyshAudit
{
    /// <summary>
    /// The object with error
    /// </summary>
    public IClyshIndexable<string> Obj { get; }
    
    /// <summary>
    /// The object errors list
    /// </summary>
    public List<string> Messages { get; }

    /// <summary>
    /// The Clysh error representation
    /// </summary>
    /// <param name="obj">The object with error</param>
    public ClyshAudit(IClyshIndexable<string> obj)
    {
        Obj = obj;
        Messages = new ();
    }

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
        var line =  $"ObjectId: {Obj.Id}, Type: {Obj.GetType()}\n";

        line += $"    Message(s): [{Messages.Aggregate("\n        ", (current, message) => $"{current}{message}\n        ")}]";
        
        return line;
    }
}