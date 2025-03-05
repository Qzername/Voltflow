using Avalonia.SimplePreferences;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Net;
using System.Net.Http;
using Voltflow.ViewModels.Account;
using Voltflow.ViewModels.Pages.Cars;
using Voltflow.ViewModels.Pages.Map;
using Voltflow.ViewModels.Pages.Statistics;

namespace Voltflow.ViewModels;

/// <summary>
/// ViewModel for MainView that acts as a router.
/// </summary>
public class MainViewModel : ReactiveObject, IScreen
{
    public RoutingState Router { get; } = new();

    [Reactive] public bool IsMobile { get; set; } = OperatingSystem.IsAndroid();
    [Reactive] public bool Authenticated { get; set; }
    [Reactive] public bool IsAdmin { get; set; }

    /// <summary>
    /// Constructor for MainViewModel.
    /// When constructed, the router navigates to MapView (if on Desktop/Browser or Mobile - authenticated) or AccountView (if on Mobile - not authenticated).
    /// </summary>
    public MainViewModel()
    {
        Initialize();
    }

    public async void Initialize()
    {
        var token = await Preferences.GetAsync<string>("token", null);
        var httpClient = GetService<HttpClient>();

        if (token is not null)
        {
            httpClient.DefaultRequestHeaders.Authorization = new("Bearer", token);

            // Check if token is valid, account deleted, etc.
            var request = await httpClient.GetAsync("/api/Accounts");

            Authenticated = request.StatusCode == HttpStatusCode.OK;
            if (!Authenticated)
            {
                await Preferences.RemoveAsync("token");
                httpClient.DefaultRequestHeaders.Authorization = null;
            }

            // Checks if user is admin.
            request = await httpClient.GetAsync("/api/Accounts/adminCheck");
            IsAdmin = request.StatusCode == HttpStatusCode.OK;
        }

        if (IsMobile && !Authenticated)
            Router.Navigate.Execute(new AccountViewModel(this));
        else
            Router.Navigate.Execute(new MapViewModel(this, Authenticated, IsAdmin));
    }

    private Type? GetCurrentViewModel() => Router.GetCurrentViewModel()?.GetType();

    public void NavigateToCars()
    {
        if (GetCurrentViewModel() == typeof(CarsViewModel))
            return;

        Router.Navigate.Execute(new CarsViewModel(this));
    }

    public void NavigateToMap()
    {
        if (GetCurrentViewModel() == typeof(MapViewModel))
            return;

        Router.Navigate.Execute(new MapViewModel(this, Authenticated, IsAdmin));
    }

    public void NavigateToAccount()
    {
        if (GetCurrentViewModel() == typeof(AccountViewModel))
            return;

        Router.Navigate.Execute(new AccountViewModel(this));
    }

    public void NavigateToStatistics()
    {
        if (GetCurrentViewModel() == typeof(StatisticsViewModel))
            return;

        Router.Navigate.Execute(new StatisticsViewModel(this, IsAdmin));
    }

    protected T GetService<T>() => Locator.Current.GetService<T>()!;
}

