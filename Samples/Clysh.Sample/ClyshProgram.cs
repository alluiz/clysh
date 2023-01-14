using Clysh.Sample;

//Use appsettings.json to control CLI create mode: Declarative or Compiled
var appSettings = AppSettingsHandler.GetAppSettings();

ICmdLineApp app = appSettings.UseDeclarativeCli ? new DeclarativeCmdLineApp() : new CompiledCmdLineApp();

app.Cli.Execute(args);