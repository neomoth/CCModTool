namespace CCModTool.Logging;

public static class LogMessage
{
	public const string LogNameVerbose = "VRB";
	public const string LogNameDebug = "DBG";
	public const string LogNameInfo = "INF";
	public const string LogNameWarning = "WRN"; 
	public const string LogNameError = "ERR";
	public const string LogNameFatal = "FTL";

	public static string LogLevelToName(LogLevel level) =>
		level switch
		{
			LogLevel.Verbose => LogNameVerbose,
			LogLevel.Debug => LogNameDebug,
			LogLevel.Info => LogNameInfo,
			LogLevel.Warn => LogNameWarning,
			LogLevel.Error => LogNameError,
			LogLevel.Fatal => LogNameFatal,
			_ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
		};
}