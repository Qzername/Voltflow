using System.Text.RegularExpressions;

namespace Voltflow.Models;

/// <summary>
/// Validates a password using rules provided by PG.
/// I added a rule to check if password is longer than 32 characters.
/// 32 characters is secure enough in my opinion. :)
/// </summary>
public static class PasswordValidator
{
    /*
	 * Password requirements
	 *
	 * Minimum eight characters,
	 * Maximum 32 characters,
	 * at least one uppercase letter,
	 * one lowercase letter,
	 * one number
	 * one special character
	 */
    public const string PasswordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,32}$";
    // Copied from VoltflowAPI.

    public static bool IsValid(string? password)
    {
        if (password == null)
            return false;

        return Regex.IsMatch(password, PasswordRegex);
    }
}

