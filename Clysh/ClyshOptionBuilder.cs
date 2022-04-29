namespace Clysh;

public class ClyshOptionBuilder
{
    private ClyshOption option;

    public ClyshOptionBuilder(string id)
    {
        option = new ClyshOption(id);
    }

    public ClyshOptionBuilder Id(string id)
    {
        option.Id = id;
        return this;
    }

    public ClyshOptionBuilder Description(string description)
    {
        option.Description = description;
        return this;
    }
    
    public ClyshOptionBuilder Shortcut(string shortcut)
    {
        option.Shortcut = shortcut;
        return this;
    }
    
    public ClyshOptionBuilder Parameters(ClyshParameters parameters)
    {
        option.Parameters = parameters;
        return this;
    }
    
    public ClyshOption Build()
    {
        return option;
    }
}