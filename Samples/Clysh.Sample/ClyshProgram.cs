using Clysh.Sample;

//Use appsettings.json to control CLI create mode: Declarative or Compiled
var appSettingsHandler = new AppSettingsHandler("appsettings.json");

IApp app = appSettingsHandler.Config.UseDeclarativeCli ? new DeclarativeApp() : new CompiledApp();

app.Cli.Execute(args);