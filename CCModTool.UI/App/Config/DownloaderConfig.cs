using CCModTool.Abstractions.Config;

namespace CCModTool.UI.App.Config;

public sealed partial class DownloaderConfig : IConfig
{
	[ConfigProp(Default = "0.35", PropSize = PropSize.Small, Tooltip = "The version of NWJS that the installer will attempt to download.")] public Version TargetNWJSVersion { get; set; }

	[ConfigProp(Default = "true", PropSize = PropSize.Small,
		Tooltip =
			"This determines whether to install the devkit for the target version or not. If false, will download the release version.")]
	public bool UseDevkit { get; set; }
	
	[ConfigProp(Default = "true", Tooltip = "This determines whether to use CCLoader or CCLoader3.\nNote: using CCLoader3 will require an additional step as CCModManager is not bundled with it.")]
	public bool UseCCLoader3 { get; set; }
	
	public long SchemaVersion { get; }
	public int Priority => 1;
}