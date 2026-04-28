using Serilog.Events;

namespace CCModTool.Logging;

public sealed class FileLogHandler : ILogHandler, IDisposable
{
	private readonly TextWriter writer;

	public FileLogHandler(string path)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		writer = TextWriter.Synchronized(new StreamWriter(
			new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read | FileShare.Delete),
			EncodingHelper.UTF8));
	}

	public void Dispose()
	{
		writer.Dispose();
	}

	public void Log(string sawmill, LogEvent message)
	{
		var name = LogMessage.LogLevelToName(message.Level.ToLevel());
		writer.WriteLine("{0:o}, [{1}] {2}: {3}", DateTime.Now, name, sawmill, message.RenderMessage());
		if(message.Exception is not null)
			writer.WriteLine(message.Exception.ToString());
		
		writer.Flush();
	}
}