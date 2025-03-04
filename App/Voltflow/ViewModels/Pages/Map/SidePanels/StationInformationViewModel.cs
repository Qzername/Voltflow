using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using Mapsui;
using Mapsui.Layers;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Ursa.Controls;
using Voltflow.Models;
using Voltflow.ViewModels.Pages.Charging;

namespace Voltflow.ViewModels.Pages.Map.SidePanels;

public class StationInformationViewModel : MapSidePanelBase
{
    public WindowToastManager? ToastManager;

    private readonly HttpClient _httpClient;

    [Reactive] public bool Available { get; set; }
    [Reactive] public bool Selected { get; set; }
    [Reactive] public string ViewTitle { get; set; } = "Click on a point.";
    [Reactive] public string Status { get; set; } = "Unknown";
    [Reactive] public int Cost { get; set; }
    [Reactive] public int MaxChargeRate { get; set; }
    [Reactive] public bool ContainsPorts { get; set; }

    [Reactive] public AvaloniaList<ChargingPort> Ports { get; set; } = [];

    private ChargingStation _data;
    private ChargingPort? _selectedPort;

    public StationInformationViewModel(MemoryLayer layer, IScreen screen) : base(layer, screen)
    {
        _httpClient = GetService<HttpClient>();
    }

    public ChargingPort? SelectedPort
    {
        get => _selectedPort;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedPort, value);
            _selectedPort = value;

            SetFetching();
            ContainsPorts = Ports.Count != 0;
            UpdateUi();
        }
    }

    public override void MapClicked(MapInfoEventArgs e)
    {
        var point = (PointFeature?)e.MapInfo?.Feature;

        if (point is null || point["data"] is null)
            return;

        SetFetching();
        _data = (ChargingStation)point["data"]!;

        Ports.Clear();
        var ports = (ChargingPort[]?)point["ports"];
        if (ports?.Length > 0)
            Ports.AddRange(ports);

        ContainsPorts = Ports.Count != 0;
        // Move this here otherwise it would loop in SelectedPort { set; }
        if (ContainsPorts)
            SelectedPort = Ports[0];
        else
            UpdateUi();
    }

    private async void UpdateUi()
    {
        //check if station is open
        var request = await _httpClient.GetAsync("/api/ChargingStations/OpeningHours?stationId=" + _data.Id);

        var json = await request.Content.ReadAsStringAsync();
        var temp = JsonConverter.Deserialize<ChargingStationOpeningHours>(json)!;
        var today = ChargingStationOpeningHours.GetToday(temp);
        var now = DateTime.Now.TimeOfDay;

        Available = today[0] <= now && now <= today[1] && SelectedPort?.Status == ChargingPortStatus.Available;
        Selected =
            SelectedPort?.Status != ChargingPortStatus.OutOfService && // Is in service
            ContainsPorts; // Has charging ports
        ViewTitle = $"Existing Point (ID: {_data.Id})";
        Cost = _data.Cost;
        MaxChargeRate = _data.MaxChargeRate;

        if (today[0] > now || now > today[1])
            Status = "Closed";
        else if (SelectedPort == null)
            Status = "No Charging Ports";
        else if (SelectedPort.Status == ChargingPortStatus.OutOfService)
            Status = "Out of Service";
        else
            Status = SelectedPort.Status.ToString();
    }

    private void SetFetching()
    {
        ViewTitle = "Fetching...";
        Status = "Fetching...";
        Cost = 0;
        MaxChargeRate = 0;
        ContainsPorts = false;
        Selected = false;
    }

    public void NavigateToStatistics() => HostScreen.Router.Navigate.Execute(new StationStatisticsViewModel(_data, HostScreen));
    public async Task NavigateToCharging()
    {
        var request = await _httpClient.GetAsync("/api/Accounts");
        if (request.StatusCode != HttpStatusCode.OK)
        {
            ToastManager?.Show(
                new Toast("Sign in first!"),
                showIcon: true,
                showClose: false,
                type: NotificationType.Error,
                classes: ["Light"]);
            return;
        }

        if (SelectedPort == null)
        {
            ToastManager?.Show(
                new Toast("Select a charging port first!"),
                showIcon: true,
                showClose: false,
                type: NotificationType.Error,
                classes: ["Light"]);
            return;
        }

        HostScreen.Router.Navigate.Execute(new ChargingViewModel(_data, SelectedPort, HostScreen));
    }
}