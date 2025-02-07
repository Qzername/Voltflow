using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Voltflow.Models
{
	public enum AuthType
	{
		SignIn = 0,
		SignUp = 1,
		TwoFactor = 2
	}

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
}
