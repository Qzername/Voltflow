using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Mapsui;
using Mapsui.Layers;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Voltflow.Models;
using Voltflow.Models.Statistics;

namespace Voltflow.ViewModels.Pages.Map.SidePanels;

public class StationStatisticsViewModel : MapSidePanelBase
{
	[Reactive] public IEnumerable<ISeries> Series { get; set; } = [];

	private readonly ChargingStation _chargingStation;

	public StationStatisticsViewModel(ChargingStation currentStation, MemoryLayer layer, IScreen screen) : base(layer, screen)
	{
		_chargingStation = currentStation;
		GetStatistics();
	}

	private async void GetStatistics()
	{
		var httpClient = GetService<HttpClient>();

		var result = await httpClient.GetAsync("/api/Statistics/ChargingStations/rushHours?stationId=" + _chargingStation.Id);
		Debug.WriteLine(result.StatusCode);
		var json = await result.Content.ReadAsStringAsync();
		ChargingStationRushHours stats = JsonConverter.Deserialize<ChargingStationRushHours>(json);

		Series = new List<ISeries>()
		{
			new ColumnSeries<int>
			{
				Values = [
					stats.Monday,
					stats.Tuesday,
					stats.Wednesday,
					stats.Thursday,
					stats.Friday,
					stats.Saturday,
					stats.Sunday,
				]
			}
		};
	}

	public override void MapClicked(MapInfoEventArgs e)
	{
		HostScreen.Router.NavigateAndReset.Execute(new StationInformationViewModel(_pointsLayer, HostScreen));
	}
}