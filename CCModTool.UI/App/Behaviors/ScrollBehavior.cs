using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace CCModTool.UI.App.Behaviors;

public class ScrollBehavior
{
	public static readonly AttachedProperty<bool> EnableScrollWheelOnScrollBarProperty =
		AvaloniaProperty.RegisterAttached<ScrollBehavior, Control, bool>(
			"EnableScrollWheelOnScrollBar", false, inherits: true);

	static ScrollBehavior()
	{
		EnableScrollWheelOnScrollBarProperty.Changed.AddClassHandler<Control>((control, e) =>
		{
			if (e.NewValue is true)
			{
				control.PointerWheelChanged += OnPointerWheelChanged;
			}
			else
			{
				control.PointerWheelChanged -= OnPointerWheelChanged;
			}
		});
	}

	private static void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
	{
		if (sender is not Control control ||
		    control.FindAncestorOfType<ScrollViewer>() is not { } scrollViewer) return;
		// Scroll by a fixed amount (adjust the multiplier as needed)
		var newOffsetY = scrollViewer.Offset.Y - (e.Delta.Y * 40);
		newOffsetY = Math.Max(0, Math.Min(newOffsetY, scrollViewer.Extent.Height - scrollViewer.Viewport.Height));
        
		scrollViewer.Offset = new Vector(scrollViewer.Offset.X, newOffsetY);
		e.Handled = true;
	}

	public static void SetEnableScrollWheelOnScrollBar(Control element, bool value) =>
		element.SetValue(EnableScrollWheelOnScrollBarProperty, value);

	public static bool GetEnableScrollWheelOnScrollBar(Control element) =>
		element.GetValue(EnableScrollWheelOnScrollBarProperty);
}