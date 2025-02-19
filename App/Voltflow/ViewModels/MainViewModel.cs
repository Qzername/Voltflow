﻿using Avalonia.SimplePreferences;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using RTools_NTS.Util;
using Splat;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive;
using Voltflow.ViewModels.Account;
using Voltflow.ViewModels.Pages.Charging;
using Voltflow.ViewModels.Pages.Map;
using Voltflow.ViewModels.Pages.Statistics;

namespace Voltflow.ViewModels;

/// <summary>
/// ViewModel for MainView that acts as a router.
/// </summary>
public class MainViewModel : ReactiveObject, IScreen
{
	public RoutingState Router { get; } = new();
	public ReactiveCommand<Unit, IRoutableViewModel> GoBack => Router.NavigateBack;

	[Reactive] public bool IsMobile { get; set; } = OperatingSystem.IsAndroid();
	[Reactive] public bool Authenticated { get; set; }

	/// <summary>
	/// Constructor for MainViewModel.
	/// When constructed, the router navigates to MapView (if on Desktop/Browser) or AccountView (if on Mobile).
	/// </summary>
	public MainViewModel()
    {
		//setup httpClient
		var token = Preferences.Get<string?>("token", null);

		if(token is not null)
            Locator.Current.GetService<HttpClient>()!.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        //setup views
        if (IsMobile)
			Router.Navigate.Execute(new AccountViewModel(this));
		else
			Router.Navigate.Execute(new MapViewModel(this));
	}

	private Type? GetCurrentViewModel() => Router.GetCurrentViewModel()?.GetType();

	public void NavigateToMapView()
	{
		if (GetCurrentViewModel() == typeof(MapViewModel))
			return;

		Router.Navigate.Execute(new MapViewModel(this));
	}

	public void NavigateToAccountView()
	{
		if (GetCurrentViewModel() == typeof(AccountViewModel))
			return;

		Router.Navigate.Execute(new AccountViewModel(this));
	}

	public void NavigateToChargeView()
	{
		if (GetCurrentViewModel() == typeof(ChargingViewModel))
			return;

		Router.Navigate.Execute(new ChargingViewModel(this));
	}

	public void NavigateToStatisticsView()
	{
		if (GetCurrentViewModel() == typeof(StatisticsViewModel))
			return;

		Router.Navigate.Execute(new StatisticsViewModel(this));
	}
}

