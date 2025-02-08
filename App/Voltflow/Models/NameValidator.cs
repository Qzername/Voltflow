namespace Voltflow.Models;

/// <summary>
/// Validates names (first names, last names).
/// </summary>
public static class NameValidator
{
	public static bool IsValid(string? name)
	{
		// Checks if the name is null or empty.
		if (string.IsNullOrEmpty(name))
			return false;

		// Checks if the name has a space in it.
		// First and last names are validated separately.
		if (name.Contains(" "))
			return false;

		return true;
	}
}
