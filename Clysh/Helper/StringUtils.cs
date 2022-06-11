namespace Clysh.Helper;

/// <summary>
/// The string utils
/// </summary>
public static class ClyshStringUtils
{
    /// <summary>
    /// Verify if text is empty (null or white space)
    /// </summary>
    /// <param name="text">The text</param>
    /// <returns>The result</returns>
    public static bool IsEmpty(this string? text)
    {
        return string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text);
    }
}