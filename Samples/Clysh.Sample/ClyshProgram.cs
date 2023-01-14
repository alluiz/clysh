using Clysh.Sample;

//Use appsettings.json to control CLI create mode: Declarative or Compiled
var appSettings = AppSettingsHandler.GetAppSettings();

CmdLineApp app = appSettings.UseDeclarativeCli ? new DeclarativeCmdLineApp() : new CompiledCmdLineApp();

app.Execute(args);