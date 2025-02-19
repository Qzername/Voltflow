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
using Voltflow.ViewModels.Pages.Map.SidePanels;
using M = Mapsui;

namespace Voltflow.ViewModels.Pages.Map;

/// <summary>
/// ViewModel for MapView.
/// </summary>
/// <param name="screen"></param>
public class MapViewModel : ViewModelBase, IScreen
{
	// Dependency injection
	private readonly HttpClient _client;

	public RoutingState Router { get; }
	private MapSidePanelBase? _currentModeViewModel;
	private bool _isCreatingMode;

	// Map config
	[Reactive] public M.Map? Map { get; set; }
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

	public MapViewModel(IScreen screen) : base(screen)
	{
		_client = GetService<HttpClient>();

		Router = new RoutingState();
		Router.NavigateAndReset.Execute(new StationInformationViewModel(_pointsLayer, this));
	}

	[Reactive] public DisplayMode CurrentDisplayMode { get; set; } = DisplayMode.Mobile;

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
	}

	private void OnMapInteraction(object? sender, MapInfoEventArgs e)
	{
		UpdateModeViewModel();

		// Disable interaction on mobile
		if (e.MapInfo == null || CurrentDisplayMode == DisplayMode.Mobile) return;

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
				_currentModeViewModel = new ManageStationViewModel(_pointsLayer, this);
			else
				_currentModeViewModel = new StationInformationViewModel(_pointsLayer, this);

			Router.Navigate.Execute(_currentModeViewModel);
		}
	}

	public void ChangeMode()
	{
		_isCreatingMode = !_isCreatingMode;
		UpdateModeViewModel();
	}
}