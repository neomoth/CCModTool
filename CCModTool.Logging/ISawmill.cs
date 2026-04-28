namespace CCModTool.Logging;

public interface ISawmill
{
	/// The name of this sawmill instance. Determines the parent(s) of it and is printed alongside logs.
	string Name { get; }

	/// The minimum level of message that should be allowed through. If <see langword="null"/>, then the value of the parent sawmill instance is used.
	LogLevel? Level { get; set; }

	/// <summary>
	/// Adds a handler to handle and forward incoming messages to an output source.
	/// </summary>
	/// <param name="handler">The handler to be added.</param>
	void AddHandler(ILogHandler handler);

	/// <summary>
	/// Removes a handler from handling incoming messages.
	/// </summary>
	/// <param name="handler">The handler to be removed.</param>
	void RemoveHandler(ILogHandler handler);

	/// <summary>
	/// Returns whether the given log level meets or exceeds this sawmill's logging level, letting you know
	/// if a given message will even be logged or not given the current configuration.
	/// </summary>
	/// <remarks>
	///	This can be used to avoid logging things entirely if you know nothing is listening for it. This is primarily
	/// useful in cases in which the process of logging something adds substantial overhead.
	/// </remarks>
	/// <param name="level">The <see cref="LogLevel"/> you wish to check is enabled or not.</param>
	/// <returns>True if logging for this log level is enabled, false if not.</returns>
	bool IsLevelEnabled(LogLevel level) => true;

	/// Log a message alongside an exception, taking in a format string and format list using <see cref="FormattableString"/> syntax.
	void Log(LogLevel level, Exception? exception, string message, params object?[] args);
	
	/// Log a message, taking in a format string and format list using <see cref="FormattableString"/> syntax.
	void Log(LogLevel level, string message, params object?[] args);

	/// Log a message on the desired log level.
	void Log(LogLevel level, string message);

	/// Log a message as <see cref="LogLevel.Verbose"/>, taking in a format string and format list using <see cref="FormattableString"/> syntax.
	void Verbose(string message, params object?[] args) => Log(LogLevel.Verbose, message, args);

	/// Log a message as <see cref="LogLevel.Verbose"/>.
	void Verbose(string message) => Log(LogLevel.Verbose, message);

	/// Log a message as <see cref="LogLevel.Debug"/>, taking in a format string and format list using <see cref="FormattableString"/> syntax.
	void Debug(string message, params object?[] args);

	/// Log a message as <see cref="LogLevel.Debug"/>.
	void Debug(string message);

	/// Log a message as <see cref="LogLevel.Info"/>, taking in a format string and format list using <see cref="FormattableString"/> syntax.
	void Info(string message, params object?[] args);

	/// Log a message as <see cref="LogLevel.Info"/>.
	void Info(string message);

	/// Log a message as <see cref="LogLevel.Warn"/>, taking in a format string and format list using <see cref="FormattableString"/> syntax.
	void Warn(string message, params object?[] args);

	/// Log a message as <see cref="LogLevel.Warn"/>.
	void Warn(string message);

	/// Log a message as <see cref="LogLevel.Error"/>, taking in a format string and format list using <see cref="FormattableString"/> syntax.
	void Error(string message, params object?[] args);

	/// Log a message as <see cref="LogLevel.Error"/>.
	void Error(string message);

	/// Log a message as <see cref="LogLevel.Fatal"/>, taking in a format string and format list using <see cref="FormattableString"/> syntax.
	void Fatal(string message, params object?[] args);

	/// Log a message as <see cref="LogLevel.Fatal"/>.
	void Fatal(string message);
}