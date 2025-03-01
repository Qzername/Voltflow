using Avalonia.Collections;
using Mapsui;
using Mapsui.Layers;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Diagnostics;
using Voltflow.Models;
using Voltflow.ViewModels.Pages.Charging;

namespace Voltflow.ViewModels.Pages.Map.SidePanels;

public class StationInformationViewModel(MemoryLayer layer, IScreen screen) : MapSidePanelBase(layer, screen)
{
	[Reactive] public bool Selected { get; set; }
	[Reactive] public string ViewTitle { get; set; } = "Click on a point.";
	[Reactive] public string Status { get; set; } = "Unknown";
	[Reactive] public int Cost { get; set; }
	[Reactive] public int MaxChargeRate { get; set; }
	[Reactive] public bool ContainsPorts { get; set; }

	private ChargingStation _data;

    [Reactive] public AvaloniaList<ChargingPort> Ports { get; set; } = [];

	private ChargingPort? _selectedPort;
    public ChargingPort? SelectedPort 
	{
		get => _selectedPort;
		set
		{
			this.RaiseAndSetIfChanged(ref _selectedPort, value);
            _selectedPort = value;

            UpdateUi(false);
        }
	}

	public override void MapClicked(MapInfoEventArgs e)
	{
		var point = (PointFeature?)e.MapInfo?.Feature;

		if (point is null || point["data"] is null)
			return;

		_data = (ChargingStation)point["data"]!;

        Ports.Clear();
        var ports = (ChargingPort[]?)point["ports"];
		if (ports?.Length > 0)
			Ports.AddRange(ports);

		UpdateUi(true);

        Cost = _data.Cost;
		MaxChargeRate = _data.MaxChargeRate;
	}

	private void UpdateUi(bool setIndex)
	{
		ContainsPorts = Ports.Count != 0;

		if (ContainsPorts && setIndex)
	        SelectedPort = Ports[0];

        Selected = SelectedPort?.Status != ChargingPortStatus.OutOfService && ContainsPorts;
        ViewTitle = $"Existing Point (ID: {_data.Id})";
        Status = SelectedPort == null ? "No Charging Ports" : SelectedPort.Status == ChargingPortStatus.OutOfService ? "Out of Service" : SelectedPort.Status.ToString();
    }

	public void NavigateToStatistics() => HostScreen.Router.Navigate.Execute(new StationStatisticsViewModel(_data, _pointsLayer, HostScreen));
	public void NavigateToCharging() => HostScreen.Router.Navigate.Execute(new ChargingViewModel(_data, SelectedPort, HostScreen));
}