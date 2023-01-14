using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace Clysh.Sample;

public class AppSettingsHandler
{
    private static AppSettings? _settings;
    
    public static AppSettings GetAppSettings(string filename = "appsettings.json")
    {
        if (_settings == null) {
            
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(filename, false, true)
                .Build();

            _settings = config.GetSection("App").Get<AppSettings>() ?? throw new InvalidOperationException("Settings cannot be null.");
        }

        return _settings;
    }
}