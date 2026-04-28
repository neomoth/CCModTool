using CCModTool.Abstractions.Extensions;

namespace CCModTool.Logging;

public sealed partial class LogManager : ILogManager, IDisposable
{
	public const string SawmillProperty = "Sawmill";
	public const string Root = "root";
	private readonly Sawmill rootSawmill;
	public ISawmill RootSawmill => rootSawmill;

	private readonly Dictionary<string, Sawmill> sawmills = [];
	private readonly ReaderWriterLockSlim _sawmillsLock = new();

	public ISawmill GetSawmill(string name)
	{
		_sawmillsLock.EnterReadLock();
		try
		{
			if (sawmills.TryGetValue(name, out var sawmill))
				return sawmill;
		}
		finally
		{
			_sawmillsLock.ExitReadLock();
		}
		
		_sawmillsLock.EnterWriteLock();
		try
		{
			return GetSawmillUnlocked(name);
		}
		finally
		{
			_sawmillsLock.ExitWriteLock();
		}
	}

	public IEnumerable<ISawmill> AllSawmills
	{
		get
		{
			using var _ = _sawmillsLock.ReadGuard();
			return sawmills.Values.ToArray();
		}
	}

	public LogManager()
	{
		rootSawmill = new Sawmill(null, Root)
		{
			Level = LogLevel.Debug
		};
		sawmills[Root] = rootSawmill;
	}

	public void Dispose()
	{
		foreach (Sawmill s in sawmills.Values)
			s.Dispose();
	}

	private Sawmill GetSawmillUnlocked(string name)
	{
		if (sawmills.TryGetValue(name, out var sawmill))
			return sawmill;
		
		var index = name.LastIndexOf('.');
		var parentName = index == -1 ? Root : name.Substring(0, index);

		var parent = GetSawmillUnlocked(parentName);
		sawmill = new Sawmill(parent, name);
		sawmills.Add(name, sawmill);
		return sawmill;
	}
}