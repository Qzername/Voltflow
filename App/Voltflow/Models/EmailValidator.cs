using System.Linq;
using System.Net.Mail;

namespace Voltflow.Models;

public static class EmailValidator
{
	// https://stackoverflow.com/a/68198658
	public static bool IsValid(string? email)
	{
		if (string.IsNullOrEmpty(email))
			return false;

		if (!MailAddress.TryCreate(email, out MailAddress? mailAddress))
			return false;

		string[] hostParts = mailAddress.Host.Split(".");
		if (hostParts.Length == 1)
			return false;

		if (hostParts.Any(p => p == string.Empty))
			return false;

		if (hostParts[^1].Length < 2)
			return false;

		if (mailAddress.User.Contains(" "))
			return false;

		if (mailAddress.User.Split(".").Any(p => p == string.Empty))
			return false;

		return true;
	}
}

