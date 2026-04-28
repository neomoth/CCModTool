using System.Text;
using System.Threading;
using Avalonia.Threading;
using CCModTool.Logging;
using CCModTool.UI.App.ViewModels;
using Serilog.Events;

namespace CCModTool.UI.App.Logging;

public sealed class WindowLogHandler : ILogHandler, IDisposable
{
	private readonly MainWindowViewModel _vm;

	private readonly object _lock = new();
	private readonly StringBuilder _pending = new();

	private Timer? _timer;
	private bool _disposed;

	// how often UI flushes queued logs
	private const int FlushMs = 100;

	public WindowLogHandler(MainWindowViewModel vm)
	{
		_vm = vm;
		_timer = new Timer(Flush, null, FlushMs, FlushMs);
	}

	public void Log(string sawmillName, LogEvent message)
	{
		if (_disposed)
			return;

		var line =
			$"[{message.Timestamp:HH:mm:ss}] " +
			$"[{message.Level.ToLevel()}] " +
			$"[{sawmillName}] " +
			$"{message.RenderMessage()}";

		lock (_lock)
		{
			if (_pending.Length > 0)
				_pending.Append('\n');

			_pending.Append(line);
		}
	}

	private void Flush(object? state)
	{
		if (_disposed)
			return;

		string text;

		lock (_lock)
		{
			if (_pending.Length == 0)
				return;

			text = _pending.ToString();
			_pending.Clear();
		}

		Dispatcher.UIThread.Post(() =>
		{
			if (_disposed)
				return;

			_vm.ConsoleOutput =
				string.IsNullOrEmpty(_vm.ConsoleOutput)
					? text
					: _vm.ConsoleOutput + "\n" + text;
		});
	}

	public void Dispose()
	{
		_disposed = true;
		_timer?.Dispose();
		_timer = null;
	}
}