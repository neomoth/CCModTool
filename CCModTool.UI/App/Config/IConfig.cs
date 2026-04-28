namespace CCModTool.UI.App.Config;

public interface IConfig
{
	long SchemaVersion { get; }
	int Priority { get; }
}