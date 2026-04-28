using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace CCModTool.UI.App.Converters;

public sealed class LogLevelToColorConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		var text = value as string ?? "";
        
		if (text.Contains("ERR:") || text.Contains("FTL:"))
			return new SolidColorBrush(Colors.Red);
		if (text.Contains("WRN:"))
			return new SolidColorBrush(Colors.Yellow);
		if (text.Contains("DBG:"))
			return new SolidColorBrush(Colors.Gray);
		if (text.Contains("VRB:"))
			return new SolidColorBrush(Colors.DarkGray);
		if (text.Contains("INF:"))
			return new SolidColorBrush(Colors.LightGreen);
            
		return new SolidColorBrush(Colors.White);
	}
    
	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}