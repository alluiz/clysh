
namespace Clysh;

public class ClyshCommandBuilder
{
    private ClyshCommand command;

    public ClyshCommandBuilder(string id)
    {
        command = new ClyshCommand(id);
    }

    public ClyshCommandBuilder AddOption(ClyshOption option)
    {
        command.AvailableOptions.Add(option);
        return this;
    }

    public ClyshCommandBuilder AddCommand(IClyshCommand children)
    {
        command.Commands.Add(children.Id, children);
        return this;
    }

    public ClyshCommand Build()
    {
        return command;
    }
}