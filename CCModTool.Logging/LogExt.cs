using System.Runtime.CompilerServices;
using Serilog.Events;

namespace CCModTool.Logging;

public static class LogExt
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static LogLevel ToLevel(this LogEventLevel level) =>
		(LogLevel)level;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static LogEventLevel ToSerilog(this LogLevel level) =>
		(LogEventLevel)level;
}