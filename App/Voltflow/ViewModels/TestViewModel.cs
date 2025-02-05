using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;

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

	[Reactive] public string Lon { get; set; } = "1";
	[Reactive] public string Lat { get; set; } = "2";
	[Reactive] public bool IsHit { get; set; }

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

		Map.Info += Map_Info;

		Map.Layers.Add(_pointsLayer);

		_isConfigured = true;
	}

	private void Map_Info(object? sender, MapInfoEventArgs e)
	{
		Lon = e.MapInfo?.WorldPosition?.X.ToString();
		Lat = e.MapInfo?.WorldPosition?.Y.ToString();

		IsHit = e.MapInfo?.Feature != null;

		if (e.MapInfo?.Feature != null)
		{
			var clickedFeature = e.MapInfo.Feature;
			var name = clickedFeature["name"]?.ToString();
		}
	}

	public void EmailTest()
	{
		var client = new SmtpClient("smtp.gmail.com", 587)
		{
			Credentials = new NetworkCredential("from-mail@gmail.com", "password"),
			EnableSsl = true
		};
		client.Send("to-mail@gmail.com", "to-mail@gmail.com", "title", "body");
	}

	public void RouteToLogin() => HostScreen.Router.Navigate.Execute(new LoginViewModel(HostScreen));
}