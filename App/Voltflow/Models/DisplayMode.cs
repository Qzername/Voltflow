using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Voltflow.Models;

public enum DisplayMode
{
	Desktop = 0,
	Mobile = 1
}

public class DisplayModeConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is DisplayMode displayMode && parameter is string paramString)
			if (Enum.TryParse(paramString, out DisplayMode paramDisplayMode))
				return displayMode == paramDisplayMode;

		return false;
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		throw new NotSupportedException();
}