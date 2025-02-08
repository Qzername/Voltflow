using System.Text.RegularExpressions;

namespace Voltflow.Models;

/// <summary>
/// Validates a password using rules provided by PG.
/// I added a rule to check if password is longer than 32 characters.
/// 32 characters is secure enough in my opinion. :)
/// </summary>
public static class PasswordValidator
{
	public static bool IsValid(string? password)
	{
		// Checks if the password is null or empty.
		if (string.IsNullOrEmpty(password))
			return false;

		// Checks if the password is in range 8-32 (8 and 32 counts as well).
		if (password.Length < 8 || password.Length > 32)
			return false;

		// Checks if the password has at least one uppercase letter.
		if (!Regex.IsMatch(password, "[A-Z]"))
			return false;

		// Checks if the password has at least one lowercase letter.
		if (!Regex.IsMatch(password, "[a-z]"))
			return false;

		// Checks if the password has at least one digit.
		if (!Regex.IsMatch(password, "\\d"))
			return false;

		// Checks if the password has at least one special character.
		if (!Regex.IsMatch(password, "[!@#$%^&*]"))
			return false;

		// Checks if the password has a space in it.
		if (password.Contains(" "))
			return false;

		return true;
	}
}

