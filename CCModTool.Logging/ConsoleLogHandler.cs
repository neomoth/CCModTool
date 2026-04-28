using System.Diagnostics;
using System.Text;
using System.Text.Unicode;
using JetBrains.Annotations;
using Serilog.Events;
using TerraFX.Interop.Windows;
using Timer = System.Timers.Timer;

namespace CCModTool.Logging;

public sealed class ConsoleLogHandler : ILogHandler, IDisposable
{
	private static readonly bool WriteAnsiColors;
	
	// ReSharper disable UnusedMember.Local
	private const string AnsiCsi = "\x1b[";
	private const string AnsiFgDefault = AnsiCsi + "39m";
	private const string AnsiFgBlack = AnsiCsi + "30m";
	private const string AnsiFgRed = AnsiCsi + "31m";
	private const string AnsiFgBrightRed = AnsiCsi + "91m";
	private const string AnsiFgGreen = AnsiCsi + "32m";
	private const string AnsiFgBrightGreen = AnsiCsi + "92m";
	private const string AnsiFgYellow = AnsiCsi + "33m";
	private const string AnsiFgBrightYellow = AnsiCsi + "93m";
	private const string AnsiFgBlue = AnsiCsi + "34m";
	private const string AnsiFgBrightBlue = AnsiCsi + "94m";
	private const string AnsiFgMagenta = AnsiCsi + "35m";
	private const string AnsiFgBrightMagenta = AnsiCsi + "95m";
	private const string AnsiFgCyan = AnsiCsi + "36m";
	private const string AnsiFgBrightCyan = AnsiCsi + "96m";
	private const string AnsiFgWhite = AnsiCsi + "37m";
	private const string AnsiFgBrightWhite = AnsiCsi + "97m";
	// ReSharper restore UnusedMember.Local

	private const string LogBeforeLevel = AnsiFgDefault + "[";
	private const string LogAfterLevel = AnsiFgDefault + "] ";

	private readonly Stream _stream = new BufferedStream(Console.OpenStandardOutput(), 128 * 1024);
	private readonly StringBuilder _line = new(1024);
	private readonly Timer _timer = new(0.1);

	private bool _disposed;

	static ConsoleLogHandler()
	{
		WriteAnsiColors = !System.Console.IsOutputRedirected;

		if (WriteAnsiColors && OperatingSystem.IsWindows())
			WriteAnsiColors = WindowsConsole.TryEnableVirtualTerminalProcessing();

		try
		{
			Console.OutputEncoding = Encoding.UTF8;
		}
		catch
		{
			// die I guess
		}
	}

	public ConsoleLogHandler()
	{
		_timer.Start();
		_timer.Elapsed += (_, _) =>
		{
			lock (_stream)
				if (IsConsoleActive && !_disposed)
					_stream.Flush();
		};
	}

	[UsedImplicitly]
	public static void TryDetachFromConsoleWindow()
	{
		if(OperatingSystem.IsWindows())
			WindowsConsole.TryDetachFromConsoleWindow();
	}

	private bool IsConsoleActive => !OperatingSystem.IsWindows() || WindowsConsole.IsConsoleActive;

	public void Log(string sawmill, LogEvent message)
	{
		var level = message.Level.ToLevel();
		lock (_stream)
		{
			_line
				.Clear()
				.Append(LogLevelToString(level))
				.Append(sawmill)
				.Append(": ")
				.AppendLine(message.RenderMessage());

			if (message.Exception is not null)
				_line.AppendLine(message.Exception.ToString());

			if (Console.OutputEncoding.CodePage == WindowsConsole.NativeMethods.CodePageUtf8)
			{
				// If we can output to UTF-8 then do it.
				Span<byte> buf = stackalloc byte[1024];
				var totalChars = _line.Length;
				foreach (var chunk in _line.GetChunks())
				{
					var chunkSize = chunk.Length;
					var totalRead = 0;
					var span = chunk.Span;
					for (;;)
					{
						var finalChunk = totalRead + chunkSize >= totalChars;
						Utf8.FromUtf16(span, buf, out var read, out var wrote, isFinalBlock: finalChunk);
						_stream.Write(buf[..wrote]);
						totalRead += read;
						if (read >= chunkSize) break;

						span = span[read..];
						chunkSize -= read;
					}
				}
			}
			else
			{
				// Fallback, sure we could just do this normally but this is slower than doing it manually.
				Console.Write(_line.ToString());
			}
			
			// ReSharper disable once InvertIf
			if (level >= LogLevel.Error)
				if (IsConsoleActive)
					_stream.Flush();
		}
	}

	public void Dispose()
	{
		lock (_stream)
		{
			_disposed = true;
			_timer.Dispose();
			_stream.Dispose();
		}
	}

	internal static string LogLevelToString(LogLevel level)
	{
		if (WriteAnsiColors)
		{
			return level switch
			{
				LogLevel.Verbose => LogBeforeLevel + AnsiFgGreen + LogMessage.LogNameVerbose + LogAfterLevel,
				LogLevel.Debug => LogBeforeLevel + AnsiFgBlue + LogMessage.LogNameDebug + LogAfterLevel,
				LogLevel.Info => LogBeforeLevel + AnsiFgBrightCyan + LogMessage.LogNameInfo + LogAfterLevel,
				LogLevel.Warn => LogBeforeLevel + AnsiFgBrightYellow + LogMessage.LogNameWarning + LogAfterLevel,
				LogLevel.Error => LogBeforeLevel + AnsiFgBrightRed + LogMessage.LogNameError + LogAfterLevel,
				LogLevel.Fatal => LogBeforeLevel + AnsiFgBrightMagenta + LogMessage.LogNameFatal + LogAfterLevel,
				_ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
			};
		}

		return level switch
		{
			LogLevel.Verbose => "[" + LogMessage.LogNameVerbose + "] ",
			LogLevel.Debug => "[" + LogMessage.LogNameDebug + "] ",
			LogLevel.Info => "[" + LogMessage.LogNameInfo + "] ",
			LogLevel.Warn => "[" + LogMessage.LogNameWarning + "] ",
			LogLevel.Error => "[" + LogMessage.LogNameError + "] ",
			LogLevel.Fatal => "[" + LogMessage.LogNameFatal + "] ",
			_ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
		};
	}

	internal static class WindowsConsole
	{
		public static unsafe bool TryEnableVirtualTerminalProcessing()
		{
			try
			{
				var stdHandle = Windows.GetStdHandle(unchecked((uint)-11));
				uint mode;
				Windows.GetConsoleMode(stdHandle, &mode);
				Windows.SetConsoleMode(stdHandle, mode | 4);
				Windows.GetConsoleMode(stdHandle, &mode);
				return (mode & 4) == 4;
			}
			catch (DllNotFoundException)
			{
				return false;
			}
			catch (EntryPointNotFoundException)
			{
				return false;
			}
		}
		
		private static bool _freedConsole;
		public static bool IsConsoleActive => !_freedConsole;

		public static void TryDetachFromConsoleWindow()
		{
			if (Windows.GetConsoleWindow() == default
			    || Debugger.IsAttached
			    || Console.IsOutputRedirected
			    || Console.IsErrorRedirected
			    || Console.IsInputRedirected)
				return;
			_freedConsole = Windows.FreeConsole();
		}

		internal static class NativeMethods
		{
			public const int CodePageUtf8 = 65001;
		}
	}
}