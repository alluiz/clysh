namespace Clysh.Core.Builder;

/// <summary>
/// A builder for <see cref="ClyshGroup"/> representation model
/// </summary>
/// <seealso cref="ClyshBuilder{T}"/>
public class ClyshGroupBuilder: ClyshBuilder<ClyshGroup>
{
    /// <summary>
    /// Build the group Id
    /// </summary>
    /// <param name="id">The group id</param>
    /// <returns>This same builder instance</returns>
    public ClyshGroupBuilder Id(string id)
    {
        Result.Id = id;
        return this;
    }
}