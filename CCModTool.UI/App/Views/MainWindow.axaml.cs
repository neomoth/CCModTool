using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using CCModTool.UI.App.ViewModels;

namespace CCModTool.UI.App.Views;

public sealed partial class MainWindow : Window
{
	private Button? selectedButton;
	
	public MainWindow()
	{
		InitializeComponent();
		
		WindowTitle.PointerPressed += OnTitleBarDoubleClick;
		LeftResize.PointerPressed += (_, e) => ResizeWindow(WindowEdge.West, e);
		RightResize.PointerPressed += (_, e) => ResizeWindow(WindowEdge.East, e);
		TopResize.PointerPressed += (_, e) => ResizeWindow(WindowEdge.North, e);
		BottomResize.PointerPressed += (_, e) => ResizeWindow(WindowEdge.South, e);
		TopLeftResize.PointerPressed += (_, e) => ResizeWindow(WindowEdge.NorthWest, e);
		TopRightResize.PointerPressed += (_, e) => ResizeWindow(WindowEdge.NorthEast, e);
		BottomLeftResize.PointerPressed += (_, e) => ResizeWindow(WindowEdge.SouthWest, e);
		BottomRightResize.PointerPressed += (_, e) => ResizeWindow(WindowEdge.SouthEast, e);
		ResizeButton.Click += (s, e) =>
		{
			var state = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
			SetWindowState(s, e, state);
		};
		CloseButton.Click += (_, _) =>
		{
			if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				desktop.Shutdown();
			}
		};
		MinimizeButton.Click += (_, _) =>
		{
			WindowState = WindowState.Minimized;
		};
		
		this.GetObservable(WindowStateProperty).Subscribe(UpdateState);
		
		InstallTabButton.Click += (s, e) => SelectButton((Button)s!);
		ConfigTabButton.Click += (s, e) => SelectButton((Button)s!);
		
		Opened += (_, _) =>
		{
			SelectButton(InstallTabButton);
		};
		
		Activated += (_, _) => Focus(true);
		Deactivated += (_, _) => Focus(false);
		
		ConsoleScroll.ScrollChanged += (_, __) =>
		{
			if (ConsoleScroll == null)
				return;

			var atBottom =
				ConsoleScroll.Offset.Y >=
				ConsoleScroll.Extent.Height - ConsoleScroll.Viewport.Height - 1;

			_autoScrollEnabled = atBottom;
		};
	}
	
	private void OnTitleBarDoubleClick(object? sender, PointerPressedEventArgs e)
	{
		if (e.ClickCount == 2)
		{
			var newState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
			SetWindowState(sender, e, newState);
		}
		else
		{
			DragWindow(sender, e);
		}
	}
	
	private void DragWindow(object? sender, PointerPressedEventArgs e)
	{
		if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e);
	}

	private void ResizeWindow(WindowEdge edge, PointerPressedEventArgs e)
	{
		if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginResizeDrag(edge, e);
	}

	private void SetWindowState(object? sender, RoutedEventArgs e, WindowState state)
	{
		WindowState = state;
	}

	private void UpdateState(WindowState state)
	{
		var geometry = state == WindowState.Maximized
			? (StreamGeometry)Application.Current?.FindResource("RestoreIcon")!
			: (StreamGeometry)Application.Current?.FindResource("MaximizeIcon")!;
		ResizeIcon.Data = geometry.Clone();

		if (state != WindowState.Normal)
		{
			ContentPadding.Margin = new Thickness(0);
			MainBorder.CornerRadius = new CornerRadius(0);
			MainBorder.ClipToBounds = false;
			ToggleResizing(false);
		}
		else
		{
			ContentPadding.Margin = new Thickness(3);
			MainBorder.CornerRadius = new CornerRadius(6);
			MainBorder.ClipToBounds = true;
			ToggleResizing(true);
		}
	}

	private void ToggleResizing(bool state)
	{
		foreach (var child in Resizers.GetVisualChildren())
		{
			if (child is Border border)
			{
				border.IsEnabled = state;
			}
		}
	}
	
	private void SelectButton(Button? sender)
	{
		// remove previous highlight
		if (selectedButton != null)
		{
			selectedButton.Classes.Remove("Selected");

			var prevText = selectedButton.GetVisualDescendants()
				.OfType<TextBlock>()
				.FirstOrDefault();
			if (prevText != null)
				prevText.Text = "";
		}

		selectedButton = sender;

		// apply new highlight
		if (sender != null)
		{
			sender.Classes.Add("Selected");

			var textBlock = sender.GetVisualDescendants()
				.OfType<TextBlock>()
				.FirstOrDefault();
			if (textBlock != null)
				textBlock.Text = ">";
		}
	}
	
	private void Focus(bool focused)
	{
		if (!focused)
		{
			CloseIcon.Foreground = new SolidColorBrush(Colors.DimGray);
			ResizeIcon.Foreground = new SolidColorBrush(Colors.DimGray);
			MinimizeIcon.Foreground = new SolidColorBrush(Colors.DimGray);
			TitleText.Foreground = new SolidColorBrush(Colors.DimGray);
			return;
		}
		CloseIcon.Foreground = new SolidColorBrush(Colors.White);
		ResizeIcon.Foreground = new SolidColorBrush(Colors.White);
		MinimizeIcon.Foreground = new SolidColorBrush(Colors.White);
		TitleText.Foreground = new SolidColorBrush(Colors.White);
	}
	
	private bool _autoScrollEnabled = true;
	
	protected override void OnDataContextChanged(EventArgs e)
	{
		base.OnDataContextChanged(e);

		if (DataContext is not MainWindowViewModel vm)
			return;

		vm.PropertyChanged -= VmOnPropertyChanged;
		vm.PropertyChanged += VmOnPropertyChanged;
		vm.MainContentGrid = MainContentGrid;
	}
	
	private void VmOnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(MainWindowViewModel.ConsoleOutput))
		{
			HandleConsoleScroll();
		}
	}
	
	private void HandleConsoleScroll()
	{
		if (ConsoleScroll == null)
			return;

		// check if user is at bottom before update
		var atBottom =
			ConsoleScroll.Offset.Y >=
			ConsoleScroll.Extent.Height - ConsoleScroll.Viewport.Height - 1;

		if (atBottom || _autoScrollEnabled)
		{
			ConsoleScroll.ScrollToEnd();
			_autoScrollEnabled = true;
		}
		else
		{
			_autoScrollEnabled = false;
		}
	}
}