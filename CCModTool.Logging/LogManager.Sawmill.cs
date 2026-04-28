using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace CCModTool.Logging;

public sealed partial class LogManager
{
	private sealed class Sawmill : ISawmill, IDisposable
	{
		/// This is used as a proxy for serilog API stuff.
		private readonly Logger _logger = new LoggerConfiguration().CreateLogger();

		public string Name { get; }
		
		public Sawmill? Parent { get; }

		private bool _disposed;

		public LogLevel? Level
		{
			get;
			set
			{
				if (Name == Root && value is null)
					throw new ArgumentException("Cannot set root sawmill level to null.");
				field = value;
			}
		}

		public List<ILogHandler> Handlers { get; } = [];
		private readonly ReaderWriterLockSlim _handlerLock = new();

		public Sawmill(Sawmill? parent, string name)
		{
			Parent = parent;
			Name = name;
		}

		public void AddHandler(ILogHandler handler)
		{
			_handlerLock.EnterWriteLock();
			try
			{
				Handlers.Add(handler);
			}
			finally
			{
				_handlerLock.ExitWriteLock();
			}
		}

		public void RemoveHandler(ILogHandler handler)
		{
			_handlerLock.EnterWriteLock();
			try
			{
				Handlers.Remove(handler);
			}
			finally
			{
				_handlerLock.ExitWriteLock();
			}
		}

		public bool IsLevelEnabled(LogLevel level) =>
			level >= GetPracticalLevel();

		public void Log(LogLevel level, Exception? exception, string message, params object?[] args)
		{
			if (!_logger.BindMessageTemplate(message, args, out var parsed, out var props))
				return;

			var msg = new LogEvent(DateTimeOffset.Now, level.ToSerilog(), exception, parsed, props);

			if (!IsLevelEnabled(level))
				return;

			LogInternal(Name, msg);
		}
		
		public void Log(LogLevel level, string message, params object?[] args)
		{
			if (args.Length != 0 && message.Contains("{0"))
			{
				// Fallback for when logs are using string.Format
				message = string.Format(message, args);
				args = [];
			}

			Log(level, null, message, args);
		}

		public void Log(LogLevel level, string message) =>
			Log(level, message, []);

		public void Dispose()
		{
			_handlerLock.EnterWriteLock();
			try
			{
				_disposed = true;
				foreach(ILogHandler handler in Handlers)
					if(handler is IDisposable dispo)
						dispo.Dispose();
			}
			finally
			{
				_handlerLock.ExitWriteLock();
			}
		}

		#region Shorthands

		public void Debug(string message, params object?[] args) => Log(LogLevel.Debug, message, args);
		public void Debug(string message) => Log(LogLevel.Debug, message);
		public void Info(string message, params object?[] args) => Log(LogLevel.Info, message, args);
		public void Info(string message) => Log(LogLevel.Info, message);
		public void Warn(string message, params object?[] args) => Log(LogLevel.Warn, message, args);
		public void Warn(string message) => Log(LogLevel.Warn, message);
		public void Error(string message, params object?[] args) => Log(LogLevel.Error, message, args);
		public void Error(string message) => Log(LogLevel.Error, message);
		public void Fatal(string message, params object?[] args) => Log(LogLevel.Fatal, message, args);
		public void Fatal(string message) => Log(LogLevel.Fatal, message);
		
		#endregion

		private void LogInternal(string source, LogEvent message)
		{
			_handlerLock.EnterWriteLock();
			try
			{
				ObjectDisposedException.ThrowIf(_disposed, this);
				foreach (var handler in Handlers)
					handler.Log(source, message);
			}
			finally
			{
				_handlerLock.ExitWriteLock();
			}
			
			Parent?.LogInternal(source, message);
		}
		
		private LogLevel GetPracticalLevel()
		{
			if (Level.HasValue)
				return Level.Value;

			return Parent?.GetPracticalLevel() ?? default;
		}
	}
}