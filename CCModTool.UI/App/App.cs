using Avalonia;
using CCModTool.Abstractions.IoC;
using CCModTool.Logging;
using CCModTool.UI.App.Config;
using CCModTool.UI.Extensions;

namespace CCModTool.UI.App;

public class App
{
	[Dependency] private readonly ILogManager _log = null!;
	public static ISawmill Log { get; private set; } = null!;

	public void Run(string[] args)
	{
		Log = _log.GetSawmill("App");
		Log.Level = LogLevel.Debug;
		Log.Debug("App started.");
		BuildApp(_log).StartWithClassicDesktopLifetime(args);
	}

	private static AppBuilder BuildApp(ILogManager log)
	{
		var logLevel = IoCManager.Resolve<ConfigManager>().Get<AppConfig>().AvaloniaLogDebug
			? LogLevel.Debug
			: LogLevel.Warn;
		return AppBuilder.Configure<AppWindow>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToDelegate(str =>
			{
				log.GetSawmill("Avalonia").Log(logLevel, str); // forward to sawmill
			}, logLevel.ToAvalonia());
	}

	public static void InitDeps(IDependencyCollection deps)
	{
		deps.Register<ConfigManager>();
	}
}