using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
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
        [Reactive] public double? Longitude { get; set; }
        [Reactive] public double? Latitude { get; set; }
        [Reactive] public int? Cost { get; set; }
        [Reactive] public int? MaxChargeRate { get; set; }
        [Reactive] public string? Password { get; set; }
        [Reactive] public bool CreatingNewPoint { get; set; } = true;
        [Reactive] public bool Clicked { get; set; }

        //ports
        [Reactive] public string? NewPortName { get; set; }
        [Reactive] public AvaloniaList<ChargingPort> Ports { get; set; } = [];
        [Reactive] public ChargingPort? SelectedPort { get; set; }

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

                _ = UpdatePortStatus();
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

                _ = UpdatePortStatus();
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

                ViewTitle = $"Existing Point (ID: {data.Id})";
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
            Password = data.Password;

            NewPortName = null;

            Ports.Clear();
            var ports = (ChargingPort[]?)feature["ports"];
            if (ports?.Length > 0)
            {
                Ports.AddRange(ports);
                SelectedPort = Ports[0];
            }

            //update values, dont send updates to server
            SetOutOfServiceNoServer(SelectedPort?.Status == ChargingPortStatus.OutOfService);
            SetServiceModeNoServer(SelectedPort?.ServiceMode ?? false);

            CreatingNewPoint = false;
        }

        public void DeleteSelectedPoint()
        {
            if (_selectedPoint is not null && _selectedPoint["data"] is null)
                ((List<IFeature>)_pointsLayer.Features).Remove(_selectedPoint);
        }
        #endregion

        #region Handling ports
        public async Task AddPort()
        {
            if (string.IsNullOrEmpty(NewPortName))
            {
                ToastManager?.Show(
                    new Toast("Provide a name for the port!"),
                    showIcon: true,
                    showClose: false,
                    type: NotificationType.Error,
                    classes: ["Light"]);
                return;
            }

            var content = JsonConverter.ToStringContent(new
            {
                StationId = CurrentStation.Id,
                Name = NewPortName
            });

            var request = await _httpClient.PostAsync("/api/ChargingPorts", content);

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
                new Toast("Added charging port successfully!"),
                showIcon: true,
                showClose: false,
                type: NotificationType.Success,
                classes: ["Light"]);
        }

        public async Task DeletePort()
        {
            if (SelectedPort == null)
            {
                ToastManager?.Show(
                    new Toast("Select a port first!"),
                    showIcon: true,
                    showClose: false,
                    type: NotificationType.Error,
                    classes: ["Light"]);

                return;
            }

            var request = await _httpClient.DeleteAsync("/api/ChargingPorts/" + SelectedPort.Id);

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
                new Toast("Deleted charging port successfully!"),
                showIcon: true,
                showClose: false,
                type: NotificationType.Success,
                classes: ["Light"]);
        }

        #endregion

        #region Handling view buttons
        public async Task CreateStation()
        {
            if (Longitude == null)
            {
                ToastManager?.Show(
                    new Toast("Longitude must be above 0!"),
                    showIcon: true,
                    showClose: false,
                    type: NotificationType.Error,
                    classes: ["Light"]);
                return;
            }

            if (Latitude == null)
            {
                ToastManager?.Show(
                    new Toast("Latitude must be above 0!"),
                    showIcon: true,
                    showClose: false,
                    type: NotificationType.Error,
                    classes: ["Light"]);
                return;
            }

            if (Cost == null)
            {
                ToastManager?.Show(
                    new Toast("Cost must be above 0!"),
                    showIcon: true,
                    showClose: false,
                    type: NotificationType.Error,
                    classes: ["Light"]);
                return;
            }

            if (MaxChargeRate == null)
            {
                ToastManager?.Show(
                    new Toast("Maximum charging rate must be above 0!"),
                    showIcon: true,
                    showClose: false,
                    type: NotificationType.Error,
                    classes: ["Light"]);
                return;
            }

            if (Password == null)
            {
                ToastManager?.Show(
                    new Toast("Password cannot be empty!"),
                    showIcon: true,
                    showClose: false,
                    type: NotificationType.Error,
                    classes: ["Light"]);
                return;
            }

            StringContent content = JsonConverter.ToStringContent(new
            {
                Longitude,
                Latitude,
                Cost,
                MaxChargeRate,
                Password
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
            if (Longitude == null)
            {
                ToastManager?.Show(
                    new Toast("Longitude must be above 0!"),
                    showIcon: true,
                    showClose: false,
                    type: NotificationType.Error,
                    classes: ["Light"]);
                return;
            }

            if (Latitude == null)
            {
                ToastManager?.Show(
                    new Toast("Latitude must be above 0!"),
                    showIcon: true,
                    showClose: false,
                    type: NotificationType.Error,
                    classes: ["Light"]);
                return;
            }

            if (Cost == null)
            {
                ToastManager?.Show(
                    new Toast("Cost must be above 0!"),
                    showIcon: true,
                    showClose: false,
                    type: NotificationType.Error,
                    classes: ["Light"]);
                return;
            }

            if (MaxChargeRate == null)
            {
                ToastManager?.Show(
                    new Toast("Maximum charging rate must be above 0!"),
                    showIcon: true,
                    showClose: false,
                    type: NotificationType.Error,
                    classes: ["Light"]);
                return;
            }

            if (Password == null)
            {
                ToastManager?.Show(
                    new Toast("Password cannot be empty!"),
                    showIcon: true,
                    showClose: false,
                    type: NotificationType.Error,
                    classes: ["Light"]);
                return;
            }

            var station = CurrentStation;

            station.Longitude = (double)Longitude;
            station.Latitude = (double)Latitude;
            station.Cost = (int)Cost;
            station.MaxChargeRate = (int)MaxChargeRate;
            station.Password = Password;

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

        public async Task UpdatePortStatus()
        {
            if (SelectedPort == null)
            {
                ToastManager?.Show(
                    new Toast("Select a port first!"),
                    showIcon: true,
                    showClose: false,
                    type: NotificationType.Error,
                    classes: ["Light"]);

                return;
            }

            var selectedPort = SelectedPort;

            selectedPort.Status = IsOutOfService ? ChargingPortStatus.OutOfService : ChargingPortStatus.Available;
            selectedPort.ServiceMode = IsInServiceMode;

            var content = JsonConverter.ToStringContent(new
            {
                selectedPort.Id,
                selectedPort.Status,
                selectedPort.ServiceMode
            });

            var request = await _httpClient.PatchAsync("/api/ChargingPorts/", content);

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

            Ports[Ports.IndexOf(SelectedPort)] = selectedPort;

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
                feature["ports"] = chargingStation.Ports;

                var today = ChargingStationOpeningHours.GetToday(chargingStation.OpeningHours);
                var now = DateTime.Now.TimeOfDay;

                if (chargingStation.Ports == null)
                    feature.Styles = [Marker.Create(Marker.Red)]; // No ports exist, so station is unavailable
                else if (chargingStation.Ports.All(x => x.Status == ChargingPortStatus.OutOfService) || today[0] > now || now > today[1])
                    feature.Styles = [Marker.Create(Marker.Red)]; // All ports are out of service or station is closed
                else if (chargingStation.Ports.Any(x => x.Status == ChargingPortStatus.Available))
                    feature.Styles = [Marker.Create(Marker.Green)]; // At least one port is available
                else if (chargingStation.Ports.All(x => x.Status == ChargingPortStatus.Occupied))
                    feature.Styles = [Marker.Create(Marker.Blue)]; // All ports are occupied

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
            Password = null;
            CreatingNewPoint = false;
            Clicked = false;
            _selectedPoint = null;
            NewPortName = null;
            SelectedPort = null;
            CreatingNewPoint = true;
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
