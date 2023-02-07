namespace Clysh.Core;

/// <summary>
/// Interface to implement your own service. Useful to complex execute logic.
/// </summary>
/// <remarks><b>Warning!</b> Customizing this service needs some knowledge of the original <see cref="ClyshService"/> class. So prefer to inherit from it for small customizations.</remarks>
/// <seealso cref="ClyshService"/>
public interface IClyshService
{
    /// <summary>
    /// The root command of the service. You can only read it.
    /// </summary>
    /// <seealso cref="IClyshCommand"/>
    ClyshCommand RootCommand { get; }

    /// <summary>
    /// The view used to interact with user. Like a front-end.
    /// </summary>
    /// <seealso cref="IClyshView"/>
    IClyshView View { get; }

    /// <summary>
    /// The main method of the CLI. Use to run the CLI rules with user args.
    /// </summary>
    /// <param name="args">The user args</param>
    void Execute(IEnumerable<string> args);
}