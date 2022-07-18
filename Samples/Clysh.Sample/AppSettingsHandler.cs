using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace Clysh.Sample;

public class AppSettingsHandler
{
    private readonly string _filename;

    public AppSettings Config { get; }

    public AppSettingsHandler(string filename)
    {
        _filename = filename;
        Config = GetAppSettings();
    }
    
    private AppSettings GetAppSettings()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(_filename, false, true)
            .Build();

        var settings = config.GetSection("App").Get<AppSettings>() ?? throw new InvalidOperationException("Settings cannot be null.");
        
        return settings;
    }
}