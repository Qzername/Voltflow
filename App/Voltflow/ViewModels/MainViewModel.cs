using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using Avalonia.Controls;
using Voltflow.Models;
using Voltflow.ViewModels.Account;
using Voltflow.ViewModels.Pages.Charging;
using Voltflow.ViewModels.Pages.Map;

namespace Voltflow.ViewModels;

/// <summary>
/// ViewModel for MainView that acts as a router.
/// </summary>
public class MainViewModel : ReactiveObject, IScreen
{
	public RoutingState Router { get; } = new();
	public ReactiveCommand<Unit, IRoutableViewModel> GoBack => Router.NavigateBack;

	[Reactive] public DisplayMode CurrentDisplayMode { get; set; } = DisplayMode.Mobile;

	/// <summary>
	/// Constructor for MainViewModel.
	/// When constructed, the router navigates to TestView by default.
	/// </summary>
	public MainViewModel()
	{
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

	public void NavigateToTestCharging()
	{
		if (GetCurrentViewModel() == typeof(ChargingViewModel))
			return;

	    Router.Navigate.Execute(new ChargingViewModel(this));
	}
}

