using Mapsui.Projections;
using Mapsui;
using ReactiveUI;
using Mapsui.Extensions;
using System.Diagnostics;
using Mapsui.Layers;
using Mapsui.UI.Avalonia;
using System.Collections.Generic;
using System;

namespace Voltflow.ViewModels;

public class MainViewModel : ViewModelBase
{
    private Map _map;

    public Map Map
    {
        get => _map;
        set => this.RaiseAndSetIfChanged(ref _map, value);
    }
    
    MemoryLayer _pointsLayer = new()
    {
        Name = "Points",
        Features = new List<IFeature>(),
        IsMapInfoLayer = true,
    };

    public MainViewModel()
    {
        Debug.WriteLine("test");
    }

    public void ConfigureMap()
    {
        Map = new Map();

        Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

        var random = new Random();
        var lng = random.NextDouble() * 360 - 180; // Random longitude between -180 y 180
        var lat = random.NextDouble() * 180 - 90;  // Random latutide between -90 y 90

        var feature = new PointFeature(
            SphericalMercator.FromLonLat(lng, lat).ToMPoint()
            );
        feature["name"] = "taaaak";

        ((List<IFeature>)_pointsLayer.Features).Add(feature);
        Map.RefreshGraphics();

        Map.Layers.Add(_pointsLayer);
    }
}
