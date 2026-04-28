namespace CCModTool.Abstractions.Config;

public enum PathType
{
	File,
	Directory
}

/// Marks a string config property as wanting a file path. This enables a button to open a file picker dialog.
[AttributeUsage(AttributeTargets.Property)]
public sealed class PathProp(PathType type = PathType.File) : Attribute
{
	public PathType Type { get; } = type;
}