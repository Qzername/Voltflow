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

	private ChargingStation _data;

    [Reactive] public AvaloniaList<ChargingPort> Ports { get; set; } = new();

	ChargingPort _selectedPort;
    public ChargingPort SelectedPort 
	{
		get => _selectedPort;
		set
		{
			this.RaiseAndSetIfChanged(ref _selectedPort, value);
            _selectedPort = value;

            UpdateUI(false);
        }
	}

	public override void MapClicked(MapInfoEventArgs e)
	{
		var point = (PointFeature?)e.MapInfo?.Feature;

		if (point is null || point["data"] is null)
			return;

		_data = (ChargingStation)point["data"]!;

        Ports.Clear();
        var ports = (ChargingPort[])point["ports"]!;
        Ports.AddRange(ports);

		UpdateUI(true);

        Cost = _data.Cost;
		MaxChargeRate = _data.MaxChargeRate;
	}

	void UpdateUI(bool setIndex)
	{
		if (Ports.Count == 0)
			return;

		if(setIndex)
	        SelectedPort = Ports[0];

        Selected = SelectedPort.Status != ChargingPortStatus.OutOfService;
        ViewTitle = $"Existing point (ID: {_data.Id})";
        Status = SelectedPort.Status == ChargingPortStatus.OutOfService ? "Out of Service" : SelectedPort.Status.ToString();
    }

	public void NavigateToStatistics() => HostScreen.Router.Navigate.Execute(new StationStatisticsViewModel(_data, _pointsLayer, HostScreen));
	public void NavigateToCharging() => HostScreen.Router.Navigate.Execute(new ChargingViewModel(_data, SelectedPort, HostScreen));
}