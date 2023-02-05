using Clysh.Helper;

namespace Clysh.Core;

public interface IClyshOption: IClyshIndexable
{
    /// <summary>
    /// The description
    /// </summary>
    string Description { get; }

    /// <summary>
    /// The parameters
    /// </summary>
    ClyshParameters Parameters { get; }

    /// <summary>
    /// The shortcut
    /// </summary>
    string? Shortcut { get; }

    /// <summary>
    /// The status of option
    /// </summary>
    bool Selected { get; set; }

    /// <summary>
    /// The group
    /// </summary>
    ClyshGroup? Group { get; }

    /// <summary>
    /// Indicates if option is global
    /// </summary>
    bool IsGlobal { get; }

    /// <summary>
    /// Check the optionId
    /// </summary>
    /// <param name="id">The id to be checked</param>
    /// <returns>The result of validation</returns>
    bool Is(string id);

    /// <summary>
    /// Format option as string
    /// </summary>
    /// <returns>The option in string format</returns>
    string ToString();
}