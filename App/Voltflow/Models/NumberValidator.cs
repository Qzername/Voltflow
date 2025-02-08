using System.Text.RegularExpressions;

namespace Voltflow.Models;

public static class NumberValidator
{
	public static bool IsValid(string? number, int length)
	{
		if (string.IsNullOrEmpty(number))
			return false;

		if (number.Length != length)
			return false;

		if (number.Contains(" "))
			return false;

		if (!Regex.IsMatch(number, "[0-9]"))
			return false;

		return true;
	}
}
