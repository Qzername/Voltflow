using Mapsui;
using Mapsui.Layers;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Voltflow.Models;

namespace Voltflow.ViewModels.Pages.Map.SidePanels;

public class StationInformationViewModel(MemoryLayer layer, IScreen screen) : MapSidePanelBase(layer, screen)
{
	[Reactive] public bool Selected { get; set; }
    [Reactive] public string ViewTitle { get; set; } = "Click on a point to start.";
    [Reactive] public string Status { get; set; } = string.Empty;
    [Reactive] public int Cost { get; set; }
    [Reactive] public int MaxChargeRate { get; set; }

	public override void MapClicked(MapInfoEventArgs e)
    {
        var point = (PointFeature?)e.MapInfo?.Feature;

        if (point is null || point["data"] is null)
            return;

        var data = (ChargingStation)point["data"]!;

        Selected = true;
        ViewTitle = $"Existing point (ID: {data.Id})";
        Status = data.Status.ToString();
        Cost = data.Cost;
        MaxChargeRate = data.MaxChargeRate;
    }

    public void Charge()
    {

    }
}
