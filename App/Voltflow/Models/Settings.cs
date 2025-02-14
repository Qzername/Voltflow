using System.IO;
using Config.Net;

namespace Voltflow.Models;

public interface ISettings
{
	[Option(Alias = "token", DefaultValue = null)]
	string? Token { get; set; }
}

public static class Settings
{
	private static readonly ISettings Current = new ConfigurationBuilder<ISettings>()
		.UseJsonFile($"{Directory.GetCurrentDirectory()}/settings.json")
		.Build();

	public static string? GetToken()
	{
		try
		{
			return Current.Token;
		}
		catch
		{
			return null;
		}
	}

	public static void SetToken(string? token) => Current.Token = token;
}