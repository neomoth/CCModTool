using Serilog.Events;

namespace CCModTool.Logging;

/// <summary>
/// Format and print a message to an output source.
/// </summary>
public interface ILogHandler
{
	/// <summary>
	/// Logs a message somewhere, depending on the <see cref="ILogHandler"/> being used.
	/// </summary>
	/// <remarks>
	/// Inheritors should ensure that this is method is thread safe, as it can be called from multiple at once.
	/// </remarks>
	/// <param name="sawmillName">The name of the sawmill the message was raised on.</param>
	/// <param name="message">The message that is to be logged to the output source.</param>
	void Log(string sawmillName, LogEvent message);
}