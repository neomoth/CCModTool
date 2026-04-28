using System.Runtime.CompilerServices;
using CCModTool.Logging;

namespace CCModTool.UI.Extensions;

public static class LogExt
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Avalonia.Logging.LogEventLevel ToAvalonia(this LogLevel level) =>
		(Avalonia.Logging.LogEventLevel)level;
}