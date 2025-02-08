using System.Text.RegularExpressions;

namespace Voltflow.Models;

public static class PasswordValidator
{
	public static bool IsValid(string? password)
	{
		if (string.IsNullOrEmpty(password))
			return false;

		if (password.Length < 8 || password.Length > 32)
			return false;

		if (!Regex.IsMatch(password, "[A-Z]"))
			return false;

		if (!Regex.IsMatch(password, "[a-z]"))
			return false;

		if (!Regex.IsMatch(password, "\\d"))
			return false;

		if (!Regex.IsMatch(password, "[!@#$%^&*]"))
			return false;

		if (password.Contains(" "))
			return false;

		return true;
	}
}

