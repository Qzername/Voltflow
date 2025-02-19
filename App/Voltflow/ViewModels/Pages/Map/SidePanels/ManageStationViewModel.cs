using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Voltflow.Models;

namespace Voltflow.ViewModels.Pages.Map.SidePanels
{
	public class ManageStationViewModel : MapSidePanelBase
	{
		// Dependency injection
		private readonly HttpClient _client;

		// Data for view
		[Reactive] public string ViewTitle { get; set; } = "Click on a point/blank space to start.";
		[Reactive] public double Longitude { get; set; }
		[Reactive] public double Latitude { get; set; }
		[Reactive] public int Cost { get; set; }
		[Reactive] public int MaxChargeRate { get; set; }

		bool _isOutOfService;
		public bool IsOutOfService
		{
			get => _isOutOfService;
			set
			{
				this.RaiseAndSetIfChanged(ref _isOutOfService, value);
				_isOutOfService = value;

				// station cannot be in service when its not out of service
				if (_isOutOfService == false)
				{
					_isInServiceMode = false;
					this.RaisePropertyChanged(nameof(IsInServiceMode));
				}

				_ = UpdateStationStatus();
			}
		}

		bool _isInServiceMode;
		public bool IsInServiceMode
		{
			get => _isInServiceMode;
			set
			{
				this.RaiseAndSetIfChanged(ref _isInServiceMode, value);
				_isInServiceMode = value;

				_ = UpdateStationStatus();
			}
		}
		ChargingStation? _current;
		PointFeature? _selectedNewPoint;

		public ManageStationViewModel(MemoryLayer layer, IScreen screen) : base(layer, screen)
		{
			_client = GetService<HttpClient>();
		}

		public override void MapClicked(MapInfoEventArgs e)
		{
			var point = (PointFeature?)e.MapInfo.Feature;

			if (point?["data"] is null)
			{
				ViewTitle = "New point";
				NewPointMode(e.MapInfo.WorldPosition!);
			}
			else
			{
				var data = (ChargingStation)point["data"]!;

				ViewTitle = $"Existing point (ID: {data.Id})";
				ExistingPointMode(point);
			}
		}

		private void NewPointMode(MPoint worldPosition)
		{
			_current = null;

			if (_selectedNewPoint is null)
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

			//update values, dont send updates to server
			_isOutOfService = data.Status == ChargingStationStatus.OutOfOrder;
			_isInServiceMode = data.ServiceMode;
			this.RaisePropertyChanged(nameof(IsOutOfService));
			this.RaisePropertyChanged(nameof(IsInServiceMode));

			_current = data;
		}

		// -------------------- For View --------------------

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

		public async Task UpdateStationStatus()
		{
			StringContent content = JsonConverter.ToStringContent(new
			{
				Id = _current!.Value.Id,
				Status = IsOutOfService ? ChargingStationStatus.OutOfOrder : ChargingStationStatus.Available,
				ServiceMode = IsInServiceMode
			});
			var result = await _client.PatchAsync("/api/ChargingStations/", content);
			Debug.WriteLine(result.StatusCode);
		}

		public async Task DeleteStation()
		{
			var result = await _client.DeleteAsync("/api/ChargingStations/" + _current!.Value.Id);
			Debug.WriteLine(result.StatusCode);
		}
	}
}
