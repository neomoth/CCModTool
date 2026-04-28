using CCModTool.Abstractions.IoC;
using CCModTool.Logging;

namespace CCModTool.UI.Logging;

public static class FileLogger
{
	private const string Extension = ".log";
	private const string Latest = "latest";
	private const string LoggingDirectoryName = "CCModTool/Logs";

	public static void InitFileLogging(ILogManager log)
	{
		var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			LoggingDirectoryName);
		
		if(!Directory.Exists(logPath))
			Directory.CreateDirectory(logPath);
		var latestPath = Path.Combine(logPath, $"{Latest}{Extension}");
		var oldPath = Path.Combine(logPath, $"{DateTime.Now:yyyy-MM-dd_HH:mm:ss}{Extension}");
		if (File.Exists(latestPath))
		{
			File.Copy(latestPath, oldPath);
			File.Delete(latestPath);
		}

		var files = new DirectoryInfo(logPath).GetFiles()
			.Where(f => !string.Equals(f.Name, $"{Latest}{Extension}", StringComparison.OrdinalIgnoreCase))
			.OrderByDescending(f => f.LastWriteTimeUtc)
			.ToList();

		// Keep only the previous 5 logs. Delete old ones.
		foreach (var file in files.Skip(5))
		{
			try
			{
				file.Delete();
			}
			catch (Exception e)
			{
				log.GetSawmill("FileLogging").Log(LogLevel.Error, e, $"Could not delete file {file.FullName}");
			}
		}
		
		var fileHandler = new FileLogHandler(latestPath);
		log.RootSawmill.AddHandler(fileHandler);
	}
}