using Clysh.Data;
using Clysh.Helper;

namespace Clysh.Core;

public interface IClyshSetup
{
    /// <summary>
    /// The CLI commands
    /// </summary>
    ClyshMap<IClyshCommand> Commands { get; }

    /// <summary>
    /// The CLI Root command
    /// </summary>
    IClyshCommand RootCommand { get; }

    /// <summary>
    /// The CLI Data
    /// </summary>
    ClyshData Data { get; }
    
    /// <summary>
    /// Bind your command action
    /// </summary>
    /// <param name="commandId">The command id</param>
    /// <param name="action">The action to be executed</param>
    void BindAction(string commandId, Action<IClyshCommand, IClyshView> action);
}