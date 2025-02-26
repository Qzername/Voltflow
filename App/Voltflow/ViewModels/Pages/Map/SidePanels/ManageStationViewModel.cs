using Avalonia.Controls.Notifications;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Ursa.Controls;
using Voltflow.Models;

namespace Voltflow.ViewModels.Pages.Map.SidePanels
{
	public class ManageStationViewModel : MapSidePanelBase
	{
		public WindowToastManager? ToastManager;

		// Dependency injection
		private readonly HttpClient _httpClient;

		// Data for view
		[Reactive] public string ViewTitle { get; set; } = "Click on a point/blank space.";
		[Reactive] public double Longitude { get; set; }
		[Reactive] public double Latitude { get; set; }
		[Reactive] public int Cost { get; set; }
		[Reactive] public int MaxChargeRate { get; set; }
		[Reactive] public bool CreatingNewPoint { get; set; }
		[Reactive] public bool Clicked { get; set; }

		private bool _isOutOfService;
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

		private bool _isInServiceMode;
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

		private PointFeature? _selectedPoint;
		private ChargingStation CurrentStation
		{
			get => _selectedPoint!["data"] is null ? new ChargingStation() : (ChargingStation)_selectedPoint!["data"]!;
			set => _selectedPoint!["data"] = value;
		}

		public ManageStationViewModel(MemoryLayer layer, IScreen screen) : base(layer, screen)
		{
			_httpClient = GetService<HttpClient>();
		}

		#region Handling map clicks
		public override void MapClicked(MapInfoEventArgs e)
		{
			var point = (PointFeature?)e.MapInfo.Feature;
			Clicked = true;

			if (point?["data"] is null)
			{
				ViewTitle = "New point";
				NewPointMode(e.MapInfo.WorldPosition!);
			}
			else
			{
				//switched mode from new to existing point
				DeleteSelectedPoint();

				var data = (ChargingStation)point["data"]!;

				ViewTitle = $"Existing point (ID: {data.Id})";
				ExistingPointMode(point);
			}
		}

		private void NewPointMode(MPoint worldPosition)
		{
			if (_selectedPoint is null || _selectedPoint["data"] is not null)
			{
				_selectedPoint = new PointFeature(worldPosition);
				((List<IFeature>)_pointsLayer.Features).Add(_selectedPoint);
			}

			_selectedPoint.Point.X = worldPosition.X;
			_selectedPoint.Point.Y = worldPosition.Y;

			var lonLat = SphericalMercator.ToLonLat(_selectedPoint.Point);

			SetOutOfServiceNoServer(false);
			SetServiceModeNoServer(false);

			Longitude = lonLat.X;
			Latitude = lonLat.Y;

			CreatingNewPoint = true;
		}

		private void ExistingPointMode(PointFeature feature)
		{
			_selectedPoint = feature;

			var data = (ChargingStation)feature["data"]!;

			Longitude = data.Longitude;
			Latitude = data.Latitude;

			Cost = data.Cost;
			MaxChargeRate = data.MaxChargeRate;

			//update values, dont send updates to server
			SetOutOfServiceNoServer(data.Status == ChargingStationStatus.OutOfService);
			SetServiceModeNoServer(data.ServiceMode);

			CreatingNewPoint = false;
		}

		public void DeleteSelectedPoint()
		{
			if (_selectedPoint is not null && _selectedPoint["data"] is null)
				((List<IFeature>)_pointsLayer.Features).Remove(_selectedPoint);
		}
		#endregion

		#region Handling view buttons
		public async Task CreateStation()
		{
			StringContent content = JsonConverter.ToStringContent(new
			{
				Longitude,
				Latitude,
				Cost,
				MaxChargeRate
			});

			var request = await _httpClient.PostAsync("/api/ChargingStations", content);

			if (request.StatusCode == HttpStatusCode.Forbidden)
			{
				ToastManager?.Show(
					new Toast("Not an administrator - missing permissions!"),
					showIcon: true,
					showClose: false,
					type: NotificationType.Error,
					classes: ["Light"]);

				return;
			}

			if (request.StatusCode == HttpStatusCode.BadRequest)
			{
				ToastManager?.Show(
					new Toast("Provided values are invalid!"),
					showIcon: true,
					showClose: false,
					type: NotificationType.Error,
					classes: ["Light"]);

				return;
			}

			if (request.StatusCode != HttpStatusCode.OK)
			{
				ToastManager?.Show(
					new Toast("Unknown error!"),
					showIcon: true,
					showClose: false,
					type: NotificationType.Error,
					classes: ["Light"]);

				return;
			}

			if (_selectedPoint == null)
			{
				ToastManager?.Show(
					new Toast("Click somewhere on a map first!"),
					showIcon: true,
					showClose: false,
					type: NotificationType.Error,
					classes: ["Light"]);

				return;
			}

			await GetStations();

			ToastManager?.Show(
				new Toast("Created station successfully!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Success,
				classes: ["Light"]);
		}

		public async Task UpdateStation()
		{
			var station = CurrentStation;

			station.Longitude = Longitude;
			station.Latitude = Latitude;
			station.Cost = Cost;
			station.MaxChargeRate = MaxChargeRate;

			var content = JsonConverter.ToStringContent(station);
			var request = await _httpClient.PatchAsync("/api/ChargingStations", content);

			if (request.StatusCode == HttpStatusCode.Forbidden)
			{
				ToastManager?.Show(
					new Toast("Not an administrator - missing permissions!"),
					showIcon: true,
					showClose: false,
					type: NotificationType.Error,
					classes: ["Light"]);

				return;
			}

			if (request.StatusCode == HttpStatusCode.BadRequest)
			{
				ToastManager?.Show(
					new Toast("Provided values are invalid!"),
					showIcon: true,
					showClose: false,
					type: NotificationType.Error,
					classes: ["Light"]);

				return;
			}

			if (request.StatusCode != HttpStatusCode.OK)
			{
				ToastManager?.Show(
					new Toast("Unknown error!"),
					showIcon: true,
					showClose: false,
					type: NotificationType.Error,
					classes: ["Light"]);

				return;
			}

			await GetStations();

			ToastManager?.Show(
				new Toast("Updated station successfully!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Success,
				classes: ["Light"]);
		}

		public async Task UpdateStationStatus()
		{
			var station = CurrentStation;

			station.Status = IsOutOfService ? ChargingStationStatus.OutOfService : ChargingStationStatus.Available;
			station.ServiceMode = IsInServiceMode;

			var content = JsonConverter.ToStringContent(new
			{
				station.Id,
				station.Status,
				station.ServiceMode
			});

			var request = await _httpClient.PatchAsync("/api/ChargingStations/", content);

			if (request.StatusCode == HttpStatusCode.Forbidden)
			{
				ToastManager?.Show(
					new Toast("Not an administrator - missing permissions!"),
					showIcon: true,
					showClose: false,
					type: NotificationType.Error,
					classes: ["Light"]);

				return;
			}

			if (request.StatusCode == HttpStatusCode.BadRequest)
			{
				ToastManager?.Show(
					new Toast("Provided values are invalid!"),
					showIcon: true,
					showClose: false,
					type: NotificationType.Error,
					classes: ["Light"]);

				return;
			}

			if (request.StatusCode != HttpStatusCode.OK)
			{
				ToastManager?.Show(
					new Toast("Unknown error!"),
					showIcon: true,
					showClose: false,
					type: NotificationType.Error,
					classes: ["Light"]);

				return;
			}

			CurrentStation = station;

			ToastManager?.Show(
				new Toast("Updated station successfully!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Success,
				classes: ["Light"]);
		}

		public async Task DeleteStation()
		{
			var station = CurrentStation;

			var request = await _httpClient.DeleteAsync("/api/ChargingStations/" + station.Id);

			if (request.StatusCode == HttpStatusCode.Forbidden)
			{
				ToastManager?.Show(
					new Toast("Not an administrator - missing permissions!"),
					showIcon: true,
					showClose: false,
					type: NotificationType.Error,
					classes: ["Light"]);

				return;
			}

			if (request.StatusCode != HttpStatusCode.OK)
			{
				ToastManager?.Show(
					new Toast("Unknown error!"),
					showIcon: true,
					showClose: false,
					type: NotificationType.Error,
					classes: ["Light"]);

				return;
			}

			Clicked = false;
			if (_selectedPoint is not null)
				((List<IFeature>)_pointsLayer.Features).Remove(_selectedPoint);

			_selectedPoint = null;
			ResetInfo();

			ToastManager?.Show(
				new Toast("Deleted station successfully!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Success,
				classes: ["Light"]);
		}
		#endregion

		private async Task GetStations()
		{
			var request = await _httpClient.GetAsync("/api/ChargingStations");

			if (request.StatusCode != HttpStatusCode.OK)
				return;

			_selectedPoint = null;
			((List<IFeature>)_pointsLayer.Features).Clear();
			ResetInfo();

			var stationsJson = await request.Content.ReadAsStringAsync();
			var chargingStations = JsonConverter.Deserialize<ChargingStation[]>(stationsJson);

			var list = (List<IFeature>)_pointsLayer.Features;

			foreach (var chargingStation in chargingStations)
			{
				var point = SphericalMercator.FromLonLat(chargingStation.Longitude, chargingStation.Latitude);

				var feature = new PointFeature(point.x, point.y);
				feature["data"] = chargingStation;

				list.Add(feature);
			}
		}

		private void ResetInfo()
		{
			ViewTitle = "Click on a point/blank space.";
			Longitude = 0;
			Latitude = 0;
			Cost = 0;
			MaxChargeRate = 0;
			CreatingNewPoint = false;
			Clicked = false;
		}

		/// <summary>
		/// sets the value of OutOfService property without server notification
		/// </summary>
		private void SetOutOfServiceNoServer(bool isOutOfService)
		{
			_isOutOfService = isOutOfService;
			this.RaisePropertyChanged(nameof(IsOutOfService));
		}

		/// <summary>
		/// sets the value of ServiceMode property without server notification
		/// </summary>
		private void SetServiceModeNoServer(bool isInServiceMode)
		{
			_isInServiceMode = isInServiceMode;
			this.RaisePropertyChanged(nameof(IsInServiceMode));
		}
	}
}
