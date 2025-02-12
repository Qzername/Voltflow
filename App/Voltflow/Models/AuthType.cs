using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Voltflow.Models;

/// <summary>
/// Enum with types for AccountViewModel to know which form to display.
/// </summary>
public enum AuthType
{
	SignIn = 0,
	SignUp = 1,
	TwoFactorAuth = 2,
	PasswordReset = 3,
	EmailVerification = 4,
	SignedIn = 5 // Not a form - shows account panel.
}

/// <summary>
/// Checks if CurrentAuthType is the same as the one in a form.
/// Using this is better than having multiple booleans bound to the 'IsVisible' property of each form.
/// Also, it makes the code look cleaner. ;)
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