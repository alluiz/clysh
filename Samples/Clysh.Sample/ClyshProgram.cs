using Clysh.Sample;

//Use appsettings.json to control CLI create mode: Declarative or Compiled
var appSettings = AppSettingsHandler.GetAppSettings();

IApp app = appSettings.UseDeclarativeCli ? new DeclarativeApp() : new CompiledApp();

app.Cli.Execute(args);