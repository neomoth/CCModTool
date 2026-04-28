using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace CCModTool.UI.App.Behaviors;

public static class AutoScrollBehavior
{
	public static readonly AttachedProperty<bool> IsEnabledProperty =
		AvaloniaProperty.RegisterAttached<Control, bool>(
			"IsEnabled",
			typeof(AutoScrollBehavior));

	private static readonly AttachedProperty<bool> IsAtBottomProperty =
		AvaloniaProperty.RegisterAttached<Control, bool>(
			"IsAtBottom",
			typeof(AutoScrollBehavior));

	public static void SetIsEnabled(Control element, bool value)
		=> element.SetValue(IsEnabledProperty, value);

	public static bool GetIsEnabled(Control element)
		=> element.GetValue(IsEnabledProperty);

	private static void SetIsAtBottom(Control element, bool value)
		=> element.SetValue(IsAtBottomProperty, value);

	private static bool GetIsAtBottom(Control element)
		=> element.GetValue(IsAtBottomProperty);

	static AutoScrollBehavior()
	{
		IsEnabledProperty.Changed.Subscribe(OnEnabledChanged);
	}

	private static void OnEnabledChanged(AvaloniaPropertyChangedEventArgs<bool> e)
	{
		if (e.Sender is not ScrollViewer sv)
			return;

		if (e.NewValue.Value)
		{
			sv.ScrollChanged += OnScrollChanged;
		}
		else
		{
			sv.ScrollChanged -= OnScrollChanged;
		}
	}

	private static void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
	{
		if (sender is not ScrollViewer sv)
			return;

		var atBottom =
			sv.Offset.Y >= sv.Extent.Height - sv.Viewport.Height - 1;

		SetIsAtBottom(sv, atBottom);
	}

	public static void ScrollToBottomIfNeeded(ScrollViewer sv)
	{
		var atBottom = GetIsAtBottom(sv);

		if (atBottom)
		{
			sv.ScrollToEnd();
		}
	}
}