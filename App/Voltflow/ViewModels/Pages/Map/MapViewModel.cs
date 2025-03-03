using Avalonia.SimplePreferences;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    // Dependency injection
    private readonly HttpClient _httpClient;

    public RoutingState Router { get; }
    private MapSidePanelBase? _currentModeViewModel;
    private bool _isCreatingMode;

    // Map config
    [Reactive] public M.Map? Map { get; set; }
    private readonly MemoryLayer _pointsLayer;

    public MapViewModel(IScreen screen) : base(screen)
    {
        var bitmapId = typeof(MapViewModel).LoadBitmapId("Assets.marker.png");

        _pointsLayer = new MemoryLayer
        {
            Name = "Points",
            Features = new List<IFeature>(),
            IsMapInfoLayer = true,
            Style = new SymbolStyle
            {
                BitmapId = bitmapId,
                SymbolScale = 0.6,
                SymbolOffset = new RelativeOffset(0.0, 0.5)
            }
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

        var token = await Preferences.GetAsync<string>("token", null);
        Authenticated = token != null;

        Map = new M.Map();
        Map.Info += OnMapInteraction;
        Map.Layers.Add(M.Tiling.OpenStreetMap.CreateTileLayer());

        // Center at Poland
        var center = SphericalMercator.FromLonLat(21.0122, 52.2297);
        Map.Home = n => n.CenterOnAndZoomTo(new MPoint(center.x, center.y), n.Resolutions[6]);

        // Add points to map
        var request = await _httpClient.GetAsync("/api/ChargingStations");
        Debug.WriteLine(request.StatusCode);
        var stationsJson = await request.Content.ReadAsStringAsync();
        var chargingStations = JsonConverter.Deserialize<ChargingStation[]>(stationsJson);

        var list = (List<IFeature>)_pointsLayer.Features;

        foreach (var chargingStation in chargingStations)
        {
            var point = SphericalMercator.FromLonLat(chargingStation.Longitude, chargingStation.Latitude);

            var feature = new PointFeature(point.x, point.y);
            feature["data"] = chargingStation;

            request = await _httpClient.GetAsync("/api/ChargingPorts?stationId=" + chargingStation.Id);
            Debug.WriteLine(request.StatusCode);
            var portsJson = await request.Content.ReadAsStringAsync();
            var ports = JsonConverter.Deserialize<ChargingPort[]>(portsJson);
            feature["ports"] = ports;

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