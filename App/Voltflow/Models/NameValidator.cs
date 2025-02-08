namespace Voltflow.Models;

public static class NameValidator
{
	public static bool IsValid(string? name)
	{
		if (string.IsNullOrEmpty(name))
			return false;

		if (name.Contains(" "))
			return false;

		return true;
	}
}
