using System.Text.RegularExpressions;

namespace Voltflow.Models;

/// <summary>
/// Validates names (names, surnames).
/// </summary>
public static class NameValidator
{
    // Can only contain lower/upper case letters.
    // No spaces, no digits.
    public const string NameRegex = "^[A-Za-z]{1,100}$";

    public static bool IsValid(string? name)
    {
        // Checks if the name is null or empty.
        if (string.IsNullOrEmpty(name))
            return false;

        // Checks if the name has any digits in it.
        return Regex.IsMatch(name, NameRegex);
    }
}
