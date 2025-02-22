using Avalonia.SimplePreferences;
using Splat;
using System;
using System.Net.Http;

namespace Voltflow;

/// <summary>
/// Loads the services into the dependency resolver.
/// </summary>
internal static class Bootstraper
{
	public async static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
	{
		var token = await Preferences.GetAsync<string>("token", null);

		services.RegisterLazySingleton(() => new HttpClient()
		{
			BaseAddress = new Uri("https://voltflow-api.heapy.xyz"),
			DefaultRequestHeaders = { Authorization = string.IsNullOrEmpty(token) ? new("Bearer", token) : null }
		}, typeof(HttpClient));
	}

	/*
     * - This method is for later use in Register method
     * - Shortens the code for getting a service from the resolver
     */
	static T GetService<T>(IReadonlyDependencyResolver resolver) => resolver.GetService<T>()!;
}