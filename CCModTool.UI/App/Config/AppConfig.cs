using CCModTool.Abstractions.Config;
using CCModTool.Logging;

namespace CCModTool.UI.App.Config;

public sealed partial class AppConfig : IConfig
{
	[ConfigProp(Default = "Info", Tooltip = "Severity level of logs to output.")]
	public LogLevel LogLevel { get; set; }
	
	[ConfigProp(Default = "false",
		Tooltip =
			"Sets whether to log debug prints from Avalonia or not. Best to keep this off unless you're debugging and also very lazy. Requires a restart to take effect.")]
	public bool AvaloniaLogDebug { get; set; }

	public long SchemaVersion { get; }
	public int Priority => 99;
}