using Mapsui.Projections;
using Mapsui;
using ReactiveUI;
using Mapsui.Extensions;
using System.Diagnostics;
using Mapsui.Layers;
using System.Collections.Generic;
using System;
using System.Net.Mail;
using System.Net;
using ReactiveUI.Fody.Helpers;
using Mapsui.Styles;

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
        Style = new SymbolStyle()
        {
            Fill = new Brush(Color.Red),
            SymbolScale = 5,
        }
    };

    [Reactive] public string Lon { get; set; }
    [Reactive] public string Lat { get; set; }
    [Reactive] public bool IsHit { get; set; }

    public MainViewModel()
    {
        Lon = "1";
        Lat = "2";
    }

    public void ConfigureMap()
    {
        Map = new Map();

        Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

        var random = new Random();
        var lng = random.NextDouble() * 360 - 180; // Random longitude between -180 y 180
        var lat = random.NextDouble() * 180 - 90;  // Random latutide between -90 y 90

        var mpoint = SphericalMercator.FromLonLat(lng, lat).ToMPoint();

        var feature = new PointFeature(mpoint);
        feature["name"] = "taaaak";

        ((List<IFeature>)_pointsLayer.Features).Add(feature);
        Map.RefreshGraphics();

        Map.Info += Map_Info;

        Map.Layers.Add(_pointsLayer);
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
        client.Send("to-mail@gmail.com", "to-mail@gmail.com", "title", "testbody");
    }
}