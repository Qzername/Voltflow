using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Mapsui;
using Mapsui.Layers;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Voltflow.Models;
using Voltflow.Models.Statistics;

namespace Voltflow.ViewModels.Pages.Map.SidePanels;

public class StationStatisticsViewModel : MapSidePanelBase
{
	[Reactive] public IEnumerable<ISeries> WeekUsage { get; set; } = [];
	[Reactive] public IEnumerable<ISeries> PeekHours { get; set; } = [];

	private readonly ChargingStation _chargingStation;
	private readonly HttpClient _httpClient;

	public StationStatisticsViewModel(ChargingStation currentStation, MemoryLayer layer, IScreen screen) : base(layer, screen)
	{
		_httpClient = GetService<HttpClient>();
		_chargingStation = currentStation;
		GetStatistics();
	}

	private async void GetStatistics()
	{
		// --- week usage ---
		var request = await _httpClient.GetAsync("/api/Statistics/ChargingStations/weekUsage?stationId=" + _chargingStation.Id);
		var json = await request.Content.ReadAsStringAsync();
		ChargingStationWeekUsage weekUsage = JsonConverter.Deserialize<ChargingStationWeekUsage>(json);

		WeekUsage = new List<ISeries>()
		{
			new ColumnSeries<int>
			{
				Values = [
					weekUsage.Monday,
					weekUsage.Tuesday,
					weekUsage.Wednesday,
					weekUsage.Thursday,
					weekUsage.Friday,
					weekUsage.Saturday,
					weekUsage.Sunday,
				],
			}
		};

		// --- peek hours ---
		request = await _httpClient.GetAsync("/api/Statistics/ChargingStations/peekHours?stationId=" + _chargingStation.Id);
		json = await request.Content.ReadAsStringAsync();
		int[] peekHours = JsonConverter.Deserialize<int[]>(json)!;

        PeekHours = new List<ISeries>()
        {
            new ColumnSeries<int>
            {
                Values = peekHours,
            }
        };
    }

	public override void MapClicked(MapInfoEventArgs e) => HostScreen.Router.NavigateAndReset.Execute(new StationInformationViewModel(_pointsLayer, HostScreen));
}