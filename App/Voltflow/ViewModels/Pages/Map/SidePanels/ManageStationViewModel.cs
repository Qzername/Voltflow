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

		PointFeature? selectedPoint;
        ChargingStation CurrentStation {
			get => selectedPoint!["data"] is null ? new ChargingStation() : (ChargingStation)selectedPoint!["data"]!;
			set => selectedPoint!["data"] = value;
        }

        public ManageStationViewModel(MemoryLayer layer, IScreen screen) : base(layer, screen)
		{
			_client = GetService<HttpClient>();
		}

        #region Handling map clicks
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
                //switched mode from new to existing point
                if (selectedPoint is not null && selectedPoint["data"] is null)
                    ((List<IFeature>)_pointsLayer.Features).Remove(selectedPoint);

                var data = (ChargingStation)point["data"]!;

				ViewTitle = $"Existing point (ID: {data.Id})";
				ExistingPointMode(point);
			}
		}

	    void NewPointMode(MPoint worldPosition)
		{
			if(selectedPoint is null || selectedPoint["data"] is not null)
            {
                selectedPoint = new PointFeature(worldPosition);
                ((List<IFeature>)_pointsLayer.Features).Add(selectedPoint);
            }

            selectedPoint.Point.X = worldPosition.X;
			selectedPoint.Point.Y = worldPosition.Y;

			var lonLat = SphericalMercator.ToLonLat(selectedPoint.Point);

			SetOutOfServiceNoServer(false);
			SetServiceModeNoServer(false);

            Longitude = lonLat.X;
			Latitude = lonLat.Y;
		}

		private void ExistingPointMode(PointFeature feature)
		{
			selectedPoint = feature;

			var data = (ChargingStation)feature["data"]!;

			Longitude = data.Longitude;
			Latitude = data.Latitude;

			Cost = data.Cost;
			MaxChargeRate = data.MaxChargeRate;

			//update values, dont send updates to server
			SetOutOfServiceNoServer(data.Status == ChargingStationStatus.OutOfService);
			SetServiceModeNoServer(data.ServiceMode);
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

			var result = await _client.PostAsync("/api/ChargingStations", content);
			Debug.WriteLine(result.StatusCode);
		}

		public async Task UpdateStation()
		{
			var station = CurrentStation;

			station.Longitude = Longitude;
            station.Latitude = Latitude;
			station.Cost = Cost;
            station.MaxChargeRate = MaxChargeRate;

            StringContent content = JsonConverter.ToStringContent(station);
			var result = await _client.PatchAsync("/api/ChargingStations", content);
            Debug.WriteLine(result.StatusCode);

            CurrentStation = station;
		}

		public async Task UpdateStationStatus()
		{
			var station = CurrentStation;

			station.Status = IsOutOfService ? ChargingStationStatus.OutOfOrder : ChargingStationStatus.Available;
			station.ServiceMode = IsInServiceMode;

            StringContent content = JsonConverter.ToStringContent(new
			{
				station.Id,
				station.Status,
				station.ServiceMode
			});
			var result = await _client.PatchAsync("/api/ChargingStations/", content);
			Debug.WriteLine(result.StatusCode);

            CurrentStation = station;
        }

		public async Task DeleteStation()
		{
			var station = CurrentStation;

			var result = await _client.DeleteAsync("/api/ChargingStations/" + station.Id);
			Debug.WriteLine(result.StatusCode);
		}
        #endregion

        /// <summary>
        /// sets the value of OutOfService property without server notification
        /// </summary>
        void SetOutOfServiceNoServer(bool isOutOfService)
        {
            _isOutOfService = isOutOfService;
            this.RaisePropertyChanged(nameof(IsOutOfService));
        }

        /// <summary>
        /// sets the value of ServiceMode property without server notification
        /// </summary>
        void SetServiceModeNoServer(bool isInServiceMode)
        {
            _isInServiceMode = isInServiceMode;
            this.RaisePropertyChanged(nameof(IsInServiceMode));
        }

    }
}
