using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Voltflow.Models;

namespace Voltflow.ViewModels;

/// <summary>
/// ViewModel for MapView.
/// </summary>
/// <param name="screen"></param>
public class MapViewModel : ViewModelBase
{
    //depenedency injection
    readonly HttpClient _client;

	//map config
    [Reactive] public Map? Map { get; set; }
    bool _isConfigured;

    readonly MemoryLayer _pointsLayer = new()
	{
		Name = "Points",
		Features = new List<IFeature>(),
		IsMapInfoLayer = true,
		Style = new SymbolStyle()
		{
			Fill = new Brush(Color.Red),
		}
	};

	PointFeature selectedNewPoint;

	//data for view
	[Reactive] double longitude { get; set; }
    [Reactive] double latitude { get; set; }
	[Reactive] int cost { get; set; }
    [Reactive] int maxChargeRate { get; set; }

    public MapViewModel(IScreen screen) : base(screen)
	{
		_client = GetService<HttpClient>();	
	}

    public void ConfigureMap()
	{
		// Prevent from running multiple times, RUN ONLY ONCE!
		if (_isConfigured) return;

		Map = new Map();
        Map.Info += OnMapInteraction;

        Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        var center = SphericalMercator.FromLonLat(21.0122, 52.2297);
        Map.Home = n => n.CenterOnAndZoomTo(new MPoint(center.x, center.y), n.Resolutions[6]);
		Map.RefreshGraphics();

		Map.Layers.Add(_pointsLayer);

		_isConfigured = true;
	}

    void OnMapInteraction(object? sender, MapInfoEventArgs e)
    {
		if(selectedNewPoint is null)
		{
            selectedNewPoint = new PointFeature(e.MapInfo!.WorldPosition!);
            ((List<IFeature>)_pointsLayer.Features).Add(selectedNewPoint);
        }

		selectedNewPoint.Point.X = e.MapInfo!.WorldPosition!.X;
        selectedNewPoint.Point.Y = e.MapInfo.WorldPosition.Y;

		var lonLat = SphericalMercator.ToLonLat(selectedNewPoint.Point);

        longitude = lonLat.X;
        latitude = lonLat.Y;

        Map!.RefreshGraphics();
    }

	public async void CreateStation()
    {
        StringContent content = JsonConverter.ToStringContent(new
        {
            Longitude = longitude,
            Latitude = latitude,
            Cost = cost,
            MaxChargeRate = maxChargeRate
        });

        var result = await _client.PostAsync("/api/ChargingStations", content);
    
        Debug.WriteLine(result.StatusCode);
    }
}