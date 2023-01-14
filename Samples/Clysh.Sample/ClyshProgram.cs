using Clysh.Sample;

//Use appsettings.json to control CLI create mode: Declarative or Compiled
var appSettings = AppSettingsHandler.GetAppSettings();

//Here is implemented Declarative or Compiled App. Check these classes to show the examples
CmdLineApp app = appSettings.UseDeclarativeCli ? new DeclarativeCmdLineApp() : new CompiledCmdLineApp();

app.Execute(args);