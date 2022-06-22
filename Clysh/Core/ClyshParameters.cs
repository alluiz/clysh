using System.Linq;
using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// The parameters set
/// </summary>
public class ClyshParameters: ClyshMap<ClyshParameter>
{
    /// <summary>
    /// Indicates if all required parameters is filled
    /// </summary>
    /// <returns>The indicator</returns>
    public bool WaitingForRequired() => Values.Any(x => x.Required && x.Data == null);

    /// <summary>
    /// Indicates if all parameters is filled
    /// </summary>
    /// <returns>The indicator</returns>
    public bool WaitingForAny() => Values.Any(x => x.Data == null);

    /// <summary>
    /// The last parameter of the set
    /// </summary>
    /// <returns>The last parameter</returns>
    public ClyshParameter Last() => this.LastOrDefault().Value;

    /// <summary>
    /// Format all required parameters
    /// </summary>
    /// <returns>The formatted string</returns>
    public string RequiredToString()
    {
        var s = "";

        Values.Where(x => x.Required).ToList().ForEach(k => s += k.Id + ",");

        if (s.Length > 1)
            s = s[..^1];

        return s;
    }
    
    /// <summary>
    /// Format all parameters
    /// </summary>
    /// <returns>The formatted string</returns>
    public override string ToString()
    {
        var paramsText = "";
        var i = 0;
            
        foreach (var parameter in Values)
        {
            var type = parameter.Required ? "R" : "O";
            paramsText += $"{i}:<{parameter.Id}:{type}>{(i < Count - 1 ? ", " : "")}";
            i++;
        }

        if (Count > 0)
            paramsText = $"[{paramsText}]: {Count}";

        return paramsText;
    }
}