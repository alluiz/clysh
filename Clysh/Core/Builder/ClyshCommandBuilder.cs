using System;
using Clysh.Helper;

namespace Clysh.Core.Builder;

public class ClyshCommandBuilder: ClyshBuilder<ClyshCommand>
{
    public ClyshCommandBuilder Id(string id)
    {
        Result.Id = id;
        return this;
    }
    
    public ClyshCommandBuilder Description(string description)
    {
        Result.Description = description;
        return this;
    }
    
    public ClyshCommandBuilder Option(ClyshOption option)
    {
        Result.AddOption(option);
        return this;
    }

    public ClyshCommandBuilder Child(ClyshCommand children)
    {
        Result.AddSubCommand(children);
        return this;
    }

    public ClyshCommandBuilder Action(Action<IClyshCommand, ClyshMap<ClyshOption>, IClyshView> action)
    {
        Result.Action = action;
        return this;
    }
}