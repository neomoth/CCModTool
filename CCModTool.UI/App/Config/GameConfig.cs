using System.ComponentModel;
using System.Runtime.CompilerServices;
using CCModTool.Abstractions.Config;

namespace CCModTool.UI.App.Config;

/// Config options relating to CrossCode itself.
public sealed partial class GameConfig : IConfig
{
	[PathProp]
	[ConfigProp(Default = "", PropSize = PropSize.Large, Tooltip = "Set this to CrossCode's executable/binary.")]
	public string InstallationPath { get; set; }
	
	[ConfigProp(Default = "true", Tooltip = "If on Linux using wayland, apply a fix on install to prevent a bug regarding changing the window size.")]
	public bool ApplyWaylandWindowResizeFix { get; set; }

	public long SchemaVersion { get; }
	public int Priority => 0;
}