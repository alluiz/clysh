namespace Clysh;

public class ClyshCommandBuilder
{
    private ClyshCommand command;

    public ClyshCommandBuilder()
    {
        command = new ClyshCommand();
    }

    public ClyshCommandBuilder Option(ClyshOption option)
    {
        command.AddOption(option);
        return this;
    }

    public ClyshCommandBuilder Child(ClyshCommand children)
    {
        command.AddChild(children);
        return this;
    }

    public ClyshCommand Build()
    {
        ClyshCommand build = command;
        command = new ClyshCommand();
        return build;
    }

    public ClyshCommandBuilder Description(string description)
    {
        this.command.Description = description;
        return this;
    }

    public ClyshCommandBuilder Id(string id)
    {
        this.command.Id = id;
        return this;
    }

    public ClyshCommandBuilder Action(Action<ClyshMap<ClyshOption>, IClyshView> action)
    {
        this.command.Action = action;
        return this;
    }
}