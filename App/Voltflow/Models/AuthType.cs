using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Voltflow.Models;

/// <summary>
/// Enum with types for AuthViewModel to know which form to display.
/// </summary>
public enum AuthType
{
	SignIn = 0,
	SignUp = 1,
	TwoFactor = 2
}

/// <summary>
/// Converts AuthType to a boolean based on the parameter. This is useful for showing/hiding forms.
/// Using this is better than having a boolean bound to "IsVisible" for each form and also makes the code look cleaner. ;)
/// </summary>
public class AuthTypeConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is AuthType authType && parameter is string paramString)
			if (Enum.TryParse(paramString, out AuthType paramAuthType))
				return authType == paramAuthType;

		return false;
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}