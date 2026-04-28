using Avalonia.Data.Converters;
using System.Globalization;

namespace CCModTool.UI.App.Converters;

public sealed class ScrollVisibilityConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		return value is true ? new Avalonia.Thickness(0, 0, 4, 0) :
			new Avalonia.Thickness(0);
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}