using Clysh.Data;
using Microsoft.Extensions.Logging;

namespace Clysh.Core;

// ReSharper disable once InconsistentNaming
public class QuickCompiledClysh : IQuickClysh
{
    private readonly ClyshService _service;

    public QuickCompiledClysh(ClyshCommand rootCommand, ClyshData data, ILoggerFactory? loggerFactory = null)
    {
        var view = new ClyshView(data, logger: loggerFactory?.CreateLogger<ClyshView>());
        _service = new ClyshService(rootCommand, view, logger: loggerFactory?.CreateLogger<ClyshService>());
    }
    
    public void Execute(IEnumerable<string> args)
    {
        _service.Execute(args);
    }
}