using Clysh.Data;
using Clysh.Helper;
// ReSharper disable UnusedMemberInSuper.Global

namespace Clysh.Core;

public interface IClyshSetup
{
    /// <summary>
    /// The CLI commands
    /// </summary>
    ClyshMap<ClyshCommand> Commands { get; }

    /// <summary>
    /// The CLI Root command
    /// </summary>
    ClyshCommand RootCommand { get; }

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

    /// <summary>
    /// Bind your command action
    /// </summary>
    /// <param name="commandId">The command id</param>
    /// <param name="action">The action that implements the IClyshAction interface</param>
    public void BindAction<T>(string commandId, T action) where T : IClyshAction;
}