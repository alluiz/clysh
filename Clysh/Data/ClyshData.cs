namespace Clysh.Data;

/// <summary>
/// Class used to deserialize data from file
/// </summary>
public class ClyshData
{
    /// <summary>
    /// Create a <b>ClyshData</b> object
    /// </summary>
    public ClyshData()
    {
    }

    /// <summary>
    /// Create a <b>ClyshData</b> object
    /// </summary>
    /// <param name="title">The CLI Title</param>
    /// <param name="version">The CLI Version</param>
    public ClyshData(string title, string version)
    {
        Title = title;
        Version = version;
    }

    /// <summary>
    /// The CLI Title
    /// </summary>
    public string Title { get; set; } = default!;

    /// <summary>
    /// The CLI Version
    /// </summary>
    public string Version { get; set; } = default!;

    /// <summary>
    /// The CLI Commands list
    /// </summary>
    public List<ClyshCommandData> Commands { get; set; } = default!;
}