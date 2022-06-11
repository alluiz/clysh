namespace Clysh.Data;

/// <summary>
/// Class used to deserialize option data from file
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class ClyshOptionData
{
    /// <summary>
    /// The id of option
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// The description
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// The CLI shortcut
    /// </summary>
    public string? Shortcut { get; set; }
        
    /// <summary>
    /// The option parameters data list
    /// </summary>
    public List<ClyshParameterData>? Parameters { get; set; }
    
    /// <summary>
    /// The option group
    /// </summary>
    public string? Group { get; set; }
}