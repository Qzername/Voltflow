using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
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

