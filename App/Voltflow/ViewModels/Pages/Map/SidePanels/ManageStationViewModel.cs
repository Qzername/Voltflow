﻿using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat.ModeDetection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Voltflow.Models;

namespace Voltflow.ViewModels.Pages.Map.SidePanels
{
    public class ManageStationViewModel : MapSidePanelBase
    {
        // Dependency injection
        private readonly HttpClient _client;

        [Reactive] public double Longitude { get; set; }
        [Reactive] public double Latitude { get; set; }
        [Reactive] public int Cost { get; set; }
        [Reactive] public int MaxChargeRate { get; set; }

        // Data for view
        [Reactive] public string Mode { get; set; } = "Click on point/blank space to start";

        ChargingStation? _current;
        PointFeature? _selectedNewPoint;

        public ManageStationViewModel(MemoryLayer layer, IScreen screen) : base(layer, screen)
        {
            _client = GetService<HttpClient>();
        }

        public override void MapClicked(MapInfoEventArgs e)
        {
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


        public async Task DeleteStation()
        {
            var result = await _client.DeleteAsync("/api/ChargingStations/" + _current!.Value.Id);
            Debug.WriteLine(result.StatusCode);
        }
    }
}
