using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CCModTool.UI.App.ViewModels;
using CCModTool.UI.App.Views;

namespace CCModTool.UI.App;

public sealed class AppWindow : Application
{
	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			desktop.MainWindow = new MainWindow
			{
				DataContext = new MainWindowViewModel()
			};
		}
		
		base.OnFrameworkInitializationCompleted();
	}
}