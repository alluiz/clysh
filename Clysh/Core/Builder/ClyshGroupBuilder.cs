namespace Clysh.Core.Builder;

/// <summary>
/// A builder for <see cref="ClyshGroup"/>
/// </summary>
/// <seealso cref="ClyshBuilder{T}"/>
public class ClyshGroupBuilder: ClyshBuilder<ClyshGroup>
{
    /// <summary>
    /// Build the group Id
    /// </summary>
    /// <param name="id">The group identifier</param>
    /// <returns>An instance of <see cref="ClyshGroupBuilder"/></returns>
    public ClyshGroupBuilder Id(string id)
    {
        result.Id = id;
        return this;
    }
}