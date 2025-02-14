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
	public static readonly ISettings Current = new ConfigurationBuilder<ISettings>()
		.UseJsonFile($"{Directory.GetCurrentDirectory()}/settings.json")
		.Build();
}