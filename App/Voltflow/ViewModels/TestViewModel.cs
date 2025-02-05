using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using ReactiveUI;
using System;
using System.Collections.Generic;

namespace Voltflow.ViewModels;

public class TestViewModel(IScreen screen) : ViewModelBase(screen)
{
	private bool _isConfigured;

	private Map? _map;
	public Map? Map
	{
		get => _map;
		set => this.RaiseAndSetIfChanged(ref _map, value);
	}

	private readonly MemoryLayer _pointsLayer = new()
	{
		Name = "Points",
		Features = new List<IFeature>(),
		IsMapInfoLayer = true,
		Style = new SymbolStyle()
		{
			Fill = new Brush(Color.Red),
			SymbolScale = 5,
		}
	};

	public void ConfigureMap()
	{
		// Prevent from running multiple times, RUN ONLY ONCE!
		if (_isConfigured) return;

		Map = new Map();
		Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

		var random = new Random();
		var lng = random.NextDouble() * 360 - 180; // Random longitude between -180 y 180
		var lat = random.NextDouble() * 180 - 90;  // Random latitude between -90 y 90

		var mPoint = SphericalMercator.FromLonLat(lng, lat).ToMPoint();

		var feature = new PointFeature(mPoint);
		feature["name"] = "test_name";

		((List<IFeature>)_pointsLayer.Features).Add(feature);
		Map.RefreshGraphics();

		Map.Layers.Add(_pointsLayer);

		_isConfigured = true;
	}
}