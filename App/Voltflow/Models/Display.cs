using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Voltflow.Models;

public static class Display
{
	public static readonly int MaxMobileWidth = 800;
}

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