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
using static SkiaSharp.HarfBuzz.SKShaper;

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
	ChargingStation? current;

	//data for view
	[Reactive] string mode { get; set; }
	[Reactive] double longitude { get; set; }
	[Reactive] double latitude { get; set; }
	[Reactive] int cost { get; set; }
	[Reactive] int maxChargeRate { get; set; }

	public MapViewModel(IScreen screen) : base(screen)
	{
		_client = GetService<HttpClient>();
	}

	public async void ConfigureMap()
	{
		// Prevent from running multiple times, RUN ONLY ONCE!
		if (_isConfigured) return;

		Map = new Map();
		Map.Info += OnMapInteraction;
		Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

		//center at poland
		var center = SphericalMercator.FromLonLat(21.0122, 52.2297);
		Map.Home = n => n.CenterOnAndZoomTo(new MPoint(center.x, center.y), n.Resolutions[6]);
		
		//add points to map
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

	void OnMapInteraction(object? sender, MapInfoEventArgs e)
	{
		if(e.MapInfo is null) return;

        var point = (PointFeature)e.MapInfo.Feature;

        if (point is null || point["data"] is null)
		{
			mode = "New point mode";
            NewPointMode(e.MapInfo.WorldPosition!);
        }
		else
        {
            var data = (ChargingStation)point["data"]!;

            mode = $"Existing point mode (ID = {data.Id})";
            ExisitingPointMode(point);
        }
    }

	void NewPointMode(MPoint worldPosition)
	{
		current = null;

        if (selectedNewPoint is null)
        {
            selectedNewPoint = new PointFeature(worldPosition);
            ((List<IFeature>)_pointsLayer.Features).Add(selectedNewPoint);
        }

        selectedNewPoint.Point.X = worldPosition.X;
        selectedNewPoint.Point.Y = worldPosition.Y;

        var lonLat = SphericalMercator.ToLonLat(selectedNewPoint.Point);

        longitude = lonLat.X;
        latitude = lonLat.Y;
    }

	void ExisitingPointMode(PointFeature feature)
    {
		if(selectedNewPoint is not null)
		{
            ((List<IFeature>)_pointsLayer.Features).Remove(selectedNewPoint);
			selectedNewPoint = null!;
        }

        var data = (ChargingStation)feature["data"]!;

        longitude = data.Longitude;
        latitude = data.Latitude;

        cost = data.Cost;
        maxChargeRate = data.MaxChargeRate;
    
		current = data;
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

	public async void UpdateStation()
	{
		StringContent content = JsonConverter.ToStringContent(new ChargingStation()
		{
			Id = current!.Value.Id,
			Longitude = longitude,
			Latitude = latitude,
			Cost = cost,
			MaxChargeRate = maxChargeRate
		});
		var result = await _client.PatchAsync("/api/ChargingStations", content);
		Debug.WriteLine(result.StatusCode);
	}


    public async void DeleteStation()
	{
		var result = await _client.DeleteAsync("/api/ChargingStations/"+ current!.Value.Id);
        Debug.WriteLine(result.StatusCode);
    }
}