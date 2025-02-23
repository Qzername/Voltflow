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
	[Reactive] public string Status { get; set; } = "Unknown";
	[Reactive] public int Cost { get; set; }
	[Reactive] public int MaxChargeRate { get; set; }

	private ChargingStation _data;

	public override void MapClicked(MapInfoEventArgs e)
	{
		var point = (PointFeature?)e.MapInfo?.Feature;

		if (point is null || point["data"] is null)
			return;

		_data = (ChargingStation)point["data"]!;

		Selected = _data.Status != ChargingStationStatus.OutOfService;
		ViewTitle = $"Existing point (ID: {_data.Id})";
		Status = _data.Status == ChargingStationStatus.OutOfService ? "Out of Service" : _data.Status.ToString();
		Cost = _data.Cost;
		MaxChargeRate = _data.MaxChargeRate;
	}

	public void Charge()
	{
		HostScreen.Router.Navigate.Execute(new ChargingViewModel(_data, HostScreen));
	}
}
