using System.Text;
using Clysh.Helper;

namespace Clysh.Core;

/// <summary>
/// The parameters set
/// </summary>
[Serializable]
public sealed class ClyshParameters: ClyshMap<ClyshParameter>
{
    /// <summary>
    /// Create an empty ClyshParameters
    /// </summary>
    public ClyshParameters()
    {
        
    }

    /// <summary>
    /// Indicates if all required parameters is filled
    /// </summary>
    /// <returns>The indicator</returns>
    public bool WaitingForRequired() => Values.Any(x => x is { Required: true, Filled: false });

    /// <summary>
    /// Indicates if all parameters is filled
    /// </summary>
    /// <returns>The indicator</returns>
    public bool WaitingForAny() => Values.Any(x => !x.Filled);

    /// <summary>
    /// The last parameter of the set
    /// </summary>
    /// <returns>The last parameter</returns>
    public ClyshParameter Last() => this
        .Where(p => !p.Value.Filled)
        .MinBy(x => x.Value.Order)
        .Value;

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
        var paramsText = new StringBuilder();
        var i = 0;
            
        foreach (var parameter in Values)
        {
            var type = parameter.Required ? "Required" : "Optional";
            paramsText.Append($"<{parameter.Id}:{type}>{(i < Count - 1 ? " " : "")}");
            i++;
        }

        return paramsText.ToString();
    }
}