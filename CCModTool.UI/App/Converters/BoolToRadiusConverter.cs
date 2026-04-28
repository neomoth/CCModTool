using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace CCModTool.UI.App.Converters;

public sealed class BoolToCornerRadiusConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		bool expanded = value is true;

		return expanded
			? new CornerRadius(0)
			: new CornerRadius(0, 0, 6, 0);
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}