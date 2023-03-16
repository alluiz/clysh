using Microsoft.Extensions.Logging;

namespace Clysh.Core;

// ReSharper disable once InconsistentNaming
public class QuickClysh : IQuickClysh
{
    public QuickClysh(IClyshSetup setup, IClyshService service)
    {
        _setup = setup;
        _service = service;
    }
    
    public QuickClysh(string path, ILoggerFactory? loggerFactory = null)
    {
        _setup = new ClyshSetup(path, logger: loggerFactory?.CreateLogger<ClyshSetup>());
        var view = new ClyshView(_setup.Data, logger: loggerFactory?.CreateLogger<ClyshView>());
        _service = new ClyshService(_setup, view, logger: loggerFactory?.CreateLogger<ClyshService>());
    }

    private readonly IClyshService _service;
    private readonly IClyshSetup _setup;
    
    public void Execute(IEnumerable<string> args)
    {
        _service.Execute(args);
    }

    public void BindAction(string commandId, IClyshActionV2 action)
    {
        _setup.BindAction(commandId, action);
    }
}