using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using CCModTool.Abstractions.IoC;
using CCModTool.Logging;

namespace CCModTool.UI.App.Config;

public sealed class ConfigManager
{
	[Dependency] private readonly ILogManager _log = null!;
	
	private const string ConfigDirectoryName = "CCModTool";
	private readonly string configPath;
	private readonly ConcurrentDictionary<Type, object> _configs = [];

	private JsonSerializerOptions _serializerOpts = new()
	{
		WriteIndented = true
	};

	public ConfigManager()
	{
		configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			ConfigDirectoryName);
		Directory.CreateDirectory(configPath);
	}

	public Type[] ConfigTypes()
	{
		return AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(SafeGetTypes)
			.Where(t => typeof(IConfig).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false }).ToArray();
	}

	private IEnumerable<Type> SafeGetTypes(Assembly asm)
	{
		try
		{
			return asm.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			return ex.Types.Where(t => t is not null)!;
		}
		catch
		{
			return [];
		}
	}

	public T Get<T>() where T : class, IConfig, new()
	{
		return (T)_configs.GetOrAdd(typeof(T), _ => Load<T>());
	}
	
	public object Get(Type type)
	{
		return _configs.GetOrAdd(type, t =>
		{
			var method = typeof(ConfigManager)
				.GetMethod(nameof(Load), BindingFlags.NonPublic | BindingFlags.Instance)!
				.MakeGenericMethod(t);

			return method.Invoke(this, null)!;
		});
	}

	public void Update<T>(Action<T> mutate) where T : class, IConfig, new()
	{
		var config = Get<T>();
		mutate(config);
		SaveInternal(config);
	}

	public void Save(IConfig config) =>
		SaveInternal(config);
	
	public void Save<T>() where T : class, IConfig, new()
	{
		var config = Get<T>();
		SaveInternal(config);
	}
	
	private T Load<T>() where T : class, IConfig, new()
	{
		var path = GetConfigPath(typeof(T));

		// No file exists -> create fresh config
		if (!File.Exists(path))
		{
			var fresh = new T();
			SaveInternal(fresh);
			return fresh;
		}

		try
		{
			var json = File.ReadAllText(path);

			var loaded = JsonSerializer.Deserialize<T>(json, _serializerOpts);
			if (loaded is null)
			{
				var fresh = new T();
				SaveInternal(fresh);
				return fresh;
			}

			// Compare generated schema version
			var freshConfig = new T();

			if (loaded.SchemaVersion == freshConfig.SchemaVersion) return loaded;
			// Schema changed -> regenerate
			SaveInternal(freshConfig);
			return freshConfig;

		}
		catch
		{
			// Corrupt file / bad json / etc
			var fresh = new T();
			SaveInternal(fresh);
			return fresh;
		}
	}

	private void SaveInternal(IConfig config)
	{
		var path = GetConfigPath(config.GetType());

		var json = JsonSerializer.Serialize(
			config,
			config.GetType(),
			_serializerOpts
		);

		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.WriteAllText(path, json);

		_log.GetSawmill("Config").Info($"Saved {config.GetType().Name}");
	}

	private string GetConfigPath(Type type) =>
		Path.Combine(configPath, $"{type.Name.Replace("Config", "").ToLower()}.json");
}