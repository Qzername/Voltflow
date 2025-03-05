using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Voltflow.Models;
using Voltflow.ViewModels.Pages.Map.SidePanels;
using M = Mapsui;

namespace Voltflow.ViewModels.Pages.Map;

/// <summary>
/// ViewModel for MapView.
/// </summary>
/// <param name="screen"></param>
public class MapViewModel : ViewModelBase, IScreen
{
    [Reactive] public bool IsMobile { get; set; } = OperatingSystem.IsAndroid();
    [Reactive] public bool Authenticated { get; set; }
    [Reactive] public bool IsAdmin { get; set; }

    // Dependency injection
    private readonly HttpClient _httpClient;

    public RoutingState Router { get; }
    private MapSidePanelBase? _currentModeViewModel;
    private bool _isCreatingMode;

    // Map config
    [Reactive] public M.Map? Map { get; set; }
    private readonly MemoryLayer _pointsLayer;

    public MapViewModel(IScreen screen, bool authenticated, bool isAdmin) : base(screen)
    {
        Authenticated = authenticated;
        IsAdmin = isAdmin;

        _pointsLayer = new MemoryLayer
        {
            Name = "Points",
            Features = new List<IFeature>(),
            IsMapInfoLayer = true,
            Style = Marker.Create(Marker.Red) // Red marker is the default
        };

        _httpClient = GetService<HttpClient>();

        Router = new RoutingState();
        _currentModeViewModel = new StationInformationViewModel(_pointsLayer, HostScreen);
        Router.NavigateAndReset.Execute(_currentModeViewModel);
    }

    public async Task ConfigureMap()
    {
        // Prevent from running multiple times, RUN ONLY ONCE!
        if (Map is not null) return;

        Map = new M.Map();
        Map.Info += OnMapInteraction;
        Map.Layers.Add(M.Tiling.OpenStreetMap.CreateTileLayer());

        // Center at Poland
        var center = SphericalMercator.FromLonLat(21.0122, 52.2297);
        Map.Home = n => n.CenterOnAndZoomTo(new MPoint(center.x, center.y), n.Resolutions[6]);

        // Add points to map
        var request = await _httpClient.GetAsync("/api/ChargingStations");

        var stationsJson = await request.Content.ReadAsStringAsync();
        var chargingStations = JsonConverter.Deserialize<ChargingStation[]>(stationsJson);

        var list = (List<IFeature>)_pointsLayer.Features;

        foreach (var chargingStation in chargingStations)
        {
            var point = SphericalMercator.FromLonLat(chargingStation.Longitude, chargingStation.Latitude);

            var feature = new PointFeature(point.x, point.y);
            feature["data"] = chargingStation;

            request = await _httpClient.GetAsync("/api/ChargingPorts?stationId=" + chargingStation.Id);

            var portsJson = await request.Content.ReadAsStringAsync();
            var ports = JsonConverter.Deserialize<ChargingPort[]>(portsJson);
            feature["ports"] = ports;

            request = await _httpClient.GetAsync("/api/ChargingStations/OpeningHours?stationId=" + chargingStation.Id);

            var json = await request.Content.ReadAsStringAsync();
            var temp = JsonConverter.Deserialize<ChargingStationOpeningHours>(json)!;
            var today = ChargingStationOpeningHours.GetToday(temp);
            var now = DateTime.Now.TimeOfDay;

            if (ports == null)
                feature.Styles = [Marker.Create(Marker.Red)]; // No ports exist, so station is unavailable
            else if (ports.All(x => x.Status == ChargingPortStatus.OutOfService) || today[0] > now || now > today[1])
                feature.Styles = [Marker.Create(Marker.Red)]; // All ports are out of service or station is closed
            else if (ports.Any(x => x.Status == ChargingPortStatus.Available))
                feature.Styles = [Marker.Create(Marker.Green)]; // At least one port is available
            else if (ports.All(x => x.Status == ChargingPortStatus.Occupied))
                feature.Styles = [Marker.Create(Marker.Blue)]; // All ports are occupied

            list.Add(feature);
        }

        Map.Layers.Add(_pointsLayer);
        Map.RefreshGraphics();
    }

    private void OnMapInteraction(object? sender, MapInfoEventArgs e)
    {
        UpdateModeViewModel();

        // Disable interaction on mobile
        if (e.MapInfo == null) return;

        _currentModeViewModel?.MapClicked(e);
    }

    void UpdateModeViewModel()
    {
        //1. if view model is not created, create it
        //2. if view model is manage station, but user wants station information, change it
        //3. if view model is station information, but user wants manage station, change it
        if (_currentModeViewModel is null ||
            _currentModeViewModel is StationInformationViewModel && _isCreatingMode ||
            _currentModeViewModel is ManageStationViewModel && !_isCreatingMode)
        {
            if (_isCreatingMode)
                _currentModeViewModel = new ManageStationViewModel(_pointsLayer, HostScreen);
            else
                _currentModeViewModel = new StationInformationViewModel(_pointsLayer, HostScreen);

            Router.NavigateAndReset.Execute(_currentModeViewModel);
        }
    }

    public void ChangeMode()
    {
        if (IsMobile)
            return;

        _isCreatingMode = !_isCreatingMode;

        if (_currentModeViewModel is ManageStationViewModel viewModel)
        {
            viewModel.DeleteSelectedPoint();
            Map?.RefreshGraphics();
        }

        UpdateModeViewModel();
    }
}