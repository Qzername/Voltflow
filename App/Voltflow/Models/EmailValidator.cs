using System.Linq;
using System.Net.Mail;

namespace Voltflow.Models;

/// <summary>
/// Validates email addresses.
/// </summary>
public static class EmailValidator
{
	// https://stackoverflow.com/a/68198658
	public static bool IsValid(string? address)
	{
		// Checks if the address is null or empty.
		if (string.IsNullOrEmpty(address))
			return false;

		// Tries to create a MailAddress object from the address.
		// If it can't, the address is invalid.
		if (!MailAddress.TryCreate(address, out MailAddress? mailAddress))
			return false;

		// Checks if host of the address has a dot in it.
		// Examples (of invalid mails): 'politechnika@gdanska', 'hello@world', 'iwork@microsoft'
		// Hosts (of examples above): 'gdanska', 'world', 'microsoft'
		// These hosts don't contain any dots, so they're invalid.
		string[] hostParts = mailAddress.Host.Split(".");
		if (hostParts.Length == 1)
			return false;

		// Checks if a dot is at the start/end of the host.
		// Examples: '.heapy.xyz', 'google.com.'
		if (hostParts.Any(p => p == string.Empty))
			return false;

		// Checks if the last part of the host (TLD) is less than 2 characters.
		//       ↓ (dot that splits the address)
		// heapy . xyz ← last part (which is a TLD)
		// Examples: 'xyz', 'pl', 'com', 'de', 'eu'
		if (hostParts[^1].Length < 2)
			return false;

		// Checks if the user part of the address has a space in it.
		if (mailAddress.User.Contains(" "))
			return false;

		// Checks if the user part of the address has a dot at the start/end by splitting it with a dot.
		// If one of the parts is empty, it means there's a dot at the start/end.
		if (mailAddress.User.Split(".").Any(p => p == string.Empty))
			return false;

		return true;
	}
}

