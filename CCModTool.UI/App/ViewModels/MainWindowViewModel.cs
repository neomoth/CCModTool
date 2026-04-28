using Avalonia.Controls;
using CCModTool.Abstractions.IoC;
using CCModTool.Logging;
using CCModTool.UI.App.Logging;
using CCModTool.UI.App.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CCModTool.UI.App.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
	[Dependency] private readonly ILogManager _log = null!;
	[ObservableProperty] public partial ViewModelBase CurrentPage { get; set; }

	private readonly InstallViewModel _installPage = new();
	private readonly ConfigViewModel _configPage = new();
	
	public Grid MainContentGrid { get; set; }

	public MainWindowViewModel()
	{
		IoCManager.InjectDependencies(this);
		_log.RootSawmill.AddHandler(new WindowLogHandler(this));
		App.Log.Debug("Initialized window logger.");
		App.Log.Verbose("Note that logs from initialization before this step will not show in the app window.");
		CurrentPage = _installPage;
	}

	[RelayCommand]
	public void GoToInstall() => CurrentPage = _installPage;

	[RelayCommand]
	public void GoToConfig() => CurrentPage = _configPage;

	[ObservableProperty]
	public partial bool IsConsoleExpanded { get; set; }

	[ObservableProperty]
	public partial string ConsoleOutput { get; set; } = "";

	public string LatestConsoleLine
	{
		get
		{
			if (string.IsNullOrWhiteSpace(ConsoleOutput))
				return "";

			var lines = ConsoleOutput
				.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

			return lines.Length > 0 ? lines[^1] : "";
		}
	}

	partial void OnConsoleOutputChanged(string value)
	{
		OnPropertyChanged(nameof(LatestConsoleLine));
	}

	public bool IsConsoleCollapsed => !IsConsoleExpanded;

	[ObservableProperty]
	public partial double ConsoleMinHeight { get; set; } = 20;
	
	partial void OnIsConsoleExpandedChanged(bool value)
	{
		OnPropertyChanged(nameof(IsConsoleCollapsed));
		
		ConsoleMinHeight = value ? 150 : 0;
		if(!IsConsoleExpanded) MainContentGrid.RowDefinitions.Last().Height = GridLength.Auto;
	}

	[RelayCommand]
	void ToggleConsole()
	{
		IsConsoleExpanded = !IsConsoleExpanded;
	}
}