using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using Avalonia.SimplePreferences;
using Avalonia.Threading;
using Microsoft.AspNetCore.SignalR.Client;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Ursa.Controls;
using Voltflow.Models;
using Voltflow.ViewModels.Pages.Map;

namespace Voltflow.ViewModels.Pages.Charging;

public class ChargingViewModel : ViewModelBase
{
    public WindowToastManager? ToastManager;

    [Reactive] public ChargingStation CurrentStation { get; set; }
    [Reactive] public ChargingPort CurrentPort { get; set; }

    [Reactive] public AvaloniaList<Car> Cars { get; set; } = [];
    [Reactive] public int SelectedIndex { get; set; } = 0;
    [Reactive] public Car SelectedCar { get; set; }

    [Reactive] public bool DeclaredAmount { get; set; }
    [Reactive] public float Amount { get; set; }

    [Reactive] public string Time { get; set; } = "00:00:00";
    [Reactive] public double TotalCharge { get; set; }
    [Reactive] public double TotalCost { get; set; }

    // Started will be always true after first start.
    // It's used for displaying charging info.
    // Finished determines whether car is done charging.
    // Working determines whether functions Start() or Stop() are running.
    [Reactive] public bool Started { get; set; }
    [Reactive] public bool Finished { get; set; } = true;
    [Reactive] public bool Working { get; set; }

    private HubConnection? _connection;
    private readonly DispatcherTimer _dataUpdate = new();
    private DateTime _startTime;
    private readonly HttpClient _httpClient;

    public ChargingViewModel(ChargingStation chargingStation, ChargingPort chargingPort, IScreen screen) : base(screen)
    {
        _httpClient = GetService<HttpClient>();

        CurrentStation = chargingStation;
        CurrentPort = chargingPort;

        _dataUpdate.Interval = TimeSpan.FromMilliseconds(100);
        _dataUpdate.Tick += async (sender, e) => await UpdateData();

        GetCarsForCombobox();
    }

    private async void GetCarsForCombobox()
    {
        //get cars for combobox
        var request = await _httpClient.GetAsync("/api/Cars");
        var jsonString = await request.Content.ReadAsStringAsync();

        if (request.StatusCode != HttpStatusCode.OK)
            return;

        var cars = JsonConverter.Deserialize<Car[]>(jsonString);
        if (cars != null)
            Cars.AddRange(cars);

        if (Cars.Count > 0)
            SelectedCar = Cars[SelectedIndex]; // SelectedIndex is 0 by default.
        else
            ToastManager?.Show(
                new Toast("You don't have any cars added! You can do that in \"Cars\" page."),
                showIcon: true,
                showClose: false,
                type: NotificationType.Error,
                classes: ["Light"]);
    }

    public async Task Start()
    {
        if (Cars.Count == 0 || SelectedIndex > Cars.Count)
        {
            ToastManager?.Show(
                new Toast("Select a car first!"),
                showIcon: true,
                showClose: false,
                type: NotificationType.Error,
                classes: ["Light"]);
            return;
        }

        if (DeclaredAmount && Amount <= 0)
        {
            ToastManager?.Show(
                new Toast("Declared amount must be above 0!"),
                showIcon: true,
                showClose: false,
                type: NotificationType.Error,
                classes: ["Light"]);
            return;
        }

        if (Working)
            return;

        Working = true;
        Started = true;

        Time = "00:00:00";
        TotalCharge = 0;
        TotalCost = 0;

        var token = await Preferences.GetAsync<string?>("token", null);
        var carId = Cars[SelectedIndex].Id;

        _connection = new HubConnectionBuilder()
            .WithUrl($"https://voltflow-api.heapy.xyz/charginghub?carId={carId}&portId={CurrentPort.Id}",
                (options) => { options.AccessTokenProvider = () => Task.FromResult(token); }).Build();

        await _connection.StartAsync();
        Finished = false;

        if (_connection.State == HubConnectionState.Connected)
        {
            _startTime = DateTime.UtcNow;
            _dataUpdate.IsEnabled = true;

            ToastManager?.Show(
                new Toast("Started charging."),
                showIcon: true,
                showClose: false,
                type: NotificationType.Success,
                classes: ["Light"]);
        }
        else
        {
            ToastManager?.Show(
                new Toast("Charging port is unavailable!"),
                showIcon: true,
                showClose: false,
                type: NotificationType.Error,
                classes: ["Light"]);
        }

        Working = false;
    }

    private async Task UpdateData()
    {
        if (!_dataUpdate.IsEnabled)
            return;

        var car = Cars[SelectedIndex];
        var chargingRate = CurrentStation.MaxChargeRate > car.ChargingRate ? car.ChargingRate : CurrentStation.MaxChargeRate;

        var time = DateTime.UtcNow - _startTime;
        Time = time.ToString("hh\\:mm\\:ss");
        TotalCharge = Math.Round(chargingRate * time.TotalSeconds / 1000, 3); //per kwh
        TotalCost = Math.Round(TotalCharge * CurrentStation.Cost, 2);

        if (DeclaredAmount && TotalCharge >= Amount)
        {
            if (!_dataUpdate.IsEnabled || _connection == null)
                return;

            _dataUpdate.IsEnabled = false;

            bool isDiscount = await _connection.InvokeAsync<bool>("RequestClose");
            await _connection.StopAsync();

            HostScreen.Router.NavigateAndReset.Execute(new TransactionViewModel(CurrentStation, TotalCost, TotalCharge, isDiscount, HostScreen));

            if (App.NotificationService is not null)
                App.NotificationService.ShowNotification("Charging finished!", "You have reached your declared amount.");

            Finished = true;
        }
    }

    public async Task Stop()
    {
        if (!_dataUpdate.IsEnabled || _connection == null || Working)
        {
            NavigateHome();
            return;
        }

        Working = true;
        _dataUpdate.IsEnabled = false;

        bool isDiscount = await _connection.InvokeAsync<bool>("RequestClose");
        await _connection.StopAsync();

        HostScreen.Router.NavigateAndReset.Execute(new TransactionViewModel(CurrentStation, TotalCost, TotalCharge, isDiscount, HostScreen));

        Finished = true;
        Working = false;
    }

    //navigation
    public void NavigateHome() => HostScreen.Router.NavigateAndReset.Execute(new MapViewModel(HostScreen));
}