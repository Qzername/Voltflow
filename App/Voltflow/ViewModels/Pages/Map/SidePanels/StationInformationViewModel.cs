using Mapsui;
using Mapsui.Layers;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Voltflow.Models;
using Voltflow.ViewModels.Pages.Charging;

namespace Voltflow.ViewModels.Pages.Map.SidePanels;

public class StationInformationViewModel(MemoryLayer layer, IScreen screen) : MapSidePanelBase(layer, screen)
{
	[Reactive] public bool Selected { get; set; }
	[Reactive] public string ViewTitle { get; set; } = "Click on a point to start.";
	[Reactive] public string Status { get; set; } = string.Empty;
	[Reactive] public int Cost { get; set; }
	[Reactive] public int MaxChargeRate { get; set; }

	ChargingStation data;

	public override void MapClicked(MapInfoEventArgs e)
	{
		var point = (PointFeature?)e.MapInfo?.Feature;

		if (point is null || point["data"] is null)
			return;

		data = (ChargingStation)point["data"]!;

		Selected = true;
		ViewTitle = $"Existing point (ID: {data.Id})";
		Status = data.Status == ChargingStationStatus.OutOfService ? "Out of Service" : data.Status.ToString();
		Cost = data.Cost;
		MaxChargeRate = data.MaxChargeRate;
	}

	public void Charge()
	{
        HostScreen.Router.Navigate.Execute(new ChargingViewModel(data, HostScreen));
    }
}
