using Serilog.Events;

namespace CCModTool.Logging;

/// <remarks>
///	The value associated with the level determines the order in which messages are filtered.
/// </remarks>
public enum LogLevel
{
	Verbose = LogEventLevel.Verbose,
	Debug = LogEventLevel.Debug,
	Info = LogEventLevel.Information,
	Warn = LogEventLevel.Warning,
	Error = LogEventLevel.Error,
	Fatal = LogEventLevel.Fatal,
}