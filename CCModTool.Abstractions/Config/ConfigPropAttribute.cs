namespace CCModTool.Abstractions.Config;

public enum PropSize
{
	Small,
	Medium,
	Large,
	Fill
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class ConfigPropAttribute() : Attribute
{
	public object? Default { get; init; }
	public bool HashDefault { get; init; } = false;
	public PropSize PropSize { get; init; } = PropSize.Fill;
	public string? Tooltip { get; init; }
}