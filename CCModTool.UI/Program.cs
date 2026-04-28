using Avalonia;
using CCModTool.Abstractions.IoC;
using CCModTool.Logging;
using CCModTool.UI.App;
using CCModTool.UI.Logging;

namespace CCModTool.UI;

public static class Program
{
	public static void Main(string[] args)
	{
		var deps = IoCManager.InitThread();
		InitDeps(deps);
		IoCManager.BuildGraph();

		var log = deps.Resolve<ILogManager>();
		var consoleHandler = new ConsoleLogHandler();
		log.RootSawmill.AddHandler(consoleHandler);
		FileLogger.InitFileLogging(log);
		var app = deps.Resolve<App.App>();
		app.Run(args);
	}

	private static void InitDeps(IDependencyCollection deps)
	{
		deps.Register<ILogManager, LogManager>();
		deps.Register<App.App>();
		App.App.InitDeps(deps);
	}
}