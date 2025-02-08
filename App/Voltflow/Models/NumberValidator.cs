using System.Text.RegularExpressions;

namespace Voltflow.Models;

/// <summary>
/// Validates if provided string is a number and has the correct length.
/// </summary>
public static class NumberValidator
{
	public static bool IsValid(string? number, int length)
	{
		// Checks if the number is null or empty.
		if (string.IsNullOrEmpty(number))
			return false;

		// Checks if the number has the correct length.
		if (number.Length != length)
			return false;

		// Checks if the number has a space in it.
		if (number.Contains(" "))
			return false;

		// Checks if the number has any non-numeric characters.
		if (!Regex.IsMatch(number, "[0-9]"))
			return false;

		return true;
	}
}
