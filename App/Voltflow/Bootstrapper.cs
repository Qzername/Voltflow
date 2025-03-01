using Splat;
using System;
using System.Net.Http;
using Voltflow.Services;

namespace Voltflow;

/// <summary>
/// Loads the services into the dependency resolver.
/// </summary>
internal static class Bootstrapper
{
	public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
	{
		services.RegisterLazySingleton(() => new HttpClient()
		{
			BaseAddress = new Uri("https://voltflow-api.heapy.xyz")
		}, typeof(HttpClient));

		services.RegisterLazySingleton(() => new DialogService());
	}

	/*
     * - This method is for later use in Register method
     * - Shortens the code for getting a service from the resolver
     */
	static T GetService<T>(IReadonlyDependencyResolver resolver) => resolver.GetService<T>()!;
}