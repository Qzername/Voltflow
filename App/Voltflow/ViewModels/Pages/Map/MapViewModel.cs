using M = Mapsui;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Voltflow.Models;

namespace Voltflow.ViewModels.Pages.Map;

/// <summary>
/// ViewModel for MapView.
/// </summary>
/// <param name="screen"></param>
public class MapViewModel : ViewModelBase
{
    // Dependency injection
    private readonly HttpClient _client;

    // Map config
    [Reactive] public M.Map? Map { get; set; }
    private bool _isConfigured;
    private readonly MemoryLayer _pointsLayer = new()
    {
        Name = "Points",
        Features = new List<IFeature>(),
        IsMapInfoLayer = true,
        Style = new SymbolStyle()
        {
            Fill = new Brush(Color.Red),
        }
    };

    private PointFeature? _selectedNewPoint;
    private ChargingStation? _current;

    // Data for view
    [Reactive] public string Mode { get; set; } = "Welcome!";
    [Reactive] public double Longitude { get; set; }
    [Reactive] public double Latitude { get; set; }
    [Reactive] public int Cost { get; set; }
    [Reactive] public int MaxChargeRate { get; set; }
    [Reactive] public DisplayMode CurrentDisplayMode { get; set; } = DisplayMode.Mobile;

    public MapViewModel(IScreen screen) : base(screen)
    {
        _client = GetService<HttpClient>();
    }

    public async Task ConfigureMap()
    {
        // Prevent from running multiple times, RUN ONLY ONCE!
        if (_isConfigured) return;

        Map = new M.Map();
        Map.Info += OnMapInteraction;
        Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

        // Center at poland
        var center = SphericalMercator.FromLonLat(21.0122, 52.2297);
        Map.Home = n => n.CenterOnAndZoomTo(new MPoint(center.x, center.y), n.Resolutions[6]);

        // Add points to map
        var response = await _client.GetAsync("/api/ChargingStations");
        var stationsJson = await response.Content.ReadAsStringAsync();
        var chargingStations = JsonConverter.Deserialize<ChargingStation[]>(stationsJson);

        Debug.WriteLine(response.StatusCode);

        var list = (List<IFeature>)_pointsLayer.Features;

        foreach (var chargingStation in chargingStations)
        {
            var point = SphericalMercator.FromLonLat(chargingStation.Longitude, chargingStation.Latitude);

            var feature = new PointFeature(point.x, point.y);
            feature["data"] = chargingStation;

            list.Add(feature);
        }

        Map.Layers.Add(_pointsLayer);
        Map.RefreshGraphics();

        _isConfigured = true;
    }

    private void OnMapInteraction(object? sender, MapInfoEventArgs e)
    {
        // Disable interaction on mobile
        if (e.MapInfo == null || CurrentDisplayMode == DisplayMode.Mobile) return;

        var point = (PointFeature?)e.MapInfo.Feature;

        if (point?["data"] == null)
        {
            Mode = "New point";
            NewPointMode(e.MapInfo.WorldPosition!);
        }
        else
        {
            var data = (ChargingStation)point["data"]!;

            Mode = $"Existing point (ID: {data.Id})";
            ExistingPointMode(point);
        }
    }

    private void NewPointMode(MPoint worldPosition)
    {
        _current = null;

        if (_selectedNewPoint == null)
        {
            _selectedNewPoint = new PointFeature(worldPosition);
            ((List<IFeature>)_pointsLayer.Features).Add(_selectedNewPoint);
        }

        _selectedNewPoint.Point.X = worldPosition.X;
        _selectedNewPoint.Point.Y = worldPosition.Y;

        var lonLat = SphericalMercator.ToLonLat(_selectedNewPoint.Point);

        Longitude = lonLat.X;
        Latitude = lonLat.Y;
    }

    private void ExistingPointMode(PointFeature feature)
    {
        if (_selectedNewPoint != null)
        {
            ((List<IFeature>)_pointsLayer.Features).Remove(_selectedNewPoint);
            _selectedNewPoint = null!;
        }

        var data = (ChargingStation)feature["data"]!;

        Longitude = data.Longitude;
        Latitude = data.Latitude;

        Cost = data.Cost;
        MaxChargeRate = data.MaxChargeRate;

        _current = data;
    }

    public async Task CreateStation()
    {
        StringContent content = JsonConverter.ToStringContent(new
        {
            Longitude,
            Latitude,
            Cost,
            MaxChargeRate
        });

        var result = await _client.PostAsync("/api/ChargingStations", content);
        Debug.WriteLine(result.StatusCode);
    }

    public async Task UpdateStation()
    {
        StringContent content = JsonConverter.ToStringContent(new ChargingStation()
        {
            Id = _current!.Value.Id,
            Longitude = Longitude,
            Latitude = Latitude,
            Cost = Cost,
            MaxChargeRate = MaxChargeRate
        });
        var result = await _client.PatchAsync("/api/ChargingStations", content);
        Debug.WriteLine(result.StatusCode);
    }


    public async Task DeleteStation()
    {
        var result = await _client.DeleteAsync("/api/ChargingStations/" + _current!.Value.Id);
        Debug.WriteLine(result.StatusCode);
    }
}