using Avalonia.Controls;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.UI.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Voltflow.Views;

public partial class MainView : UserControl
{
    MemoryLayer _pointsLayer = new()
    {
        Name = "Points",
        Features = new List<IFeature>(),
        IsMapInfoLayer = true,
    };

    MapControl mapControl;

    public MainView()
    {
        InitializeComponent();

        mapControl = new MapControl();

        Content = mapControl;

        mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        
        var random = new Random();
        var lng = random.NextDouble() * 360 - 180; // Random longitude between -180 y 180
        var lat = random.NextDouble() * 180 - 90;  // Random latutide between -90 y 90

        var feature = new PointFeature(
            SphericalMercator.FromLonLat(lng, lat).ToMPoint()
            );

        feature["name"] = "taaaak";

        ((List<IFeature>)_pointsLayer.Features).Add(feature);
        mapControl?.Map!.RefreshGraphics();
        
        if (mapControl?.Map != null)
            mapControl.Map.Info += OnChangeMapInfo;

        mapControl.Map?.Layers.Add(_pointsLayer);
    }

    void OnChangeMapInfo(object? sender, MapInfoEventArgs e)
    {
        if (e.MapInfo?.Feature != null)
        {
            var clickedFeature = e.MapInfo.Feature;
            var name = clickedFeature["name"]?.ToString();

            Console.WriteLine(name);
        }
    }

}
