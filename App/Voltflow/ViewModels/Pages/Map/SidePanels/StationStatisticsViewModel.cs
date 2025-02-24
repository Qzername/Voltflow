using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using Mapsui;
using Mapsui.Layers;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Net.Http;
using Voltflow.Models;
using System.Diagnostics;
using Voltflow.Models.Statistics;
using LiveChartsCore.SkiaSharpView.VisualElements;

namespace Voltflow.ViewModels.Pages.Map.SidePanels;

public class StationStatisticsViewModel : MapSidePanelBase
{
    [Reactive] public IEnumerable<ISeries> Series { get; set; } = [];

    ChargingStation _chargingStation;

    public StationStatisticsViewModel(ChargingStation currentStation, MemoryLayer layer, IScreen screen) : base(layer, screen)
    {
        _chargingStation = currentStation;

        Debug.WriteLine("testa");

        GetStatistics();    
    }

    async void GetStatistics()
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

        Debug.WriteLine("teste");
    }

    public override void MapClicked(MapInfoEventArgs e)
    {
        HostScreen.Router.NavigateAndReset.Execute(new StationInformationViewModel(_pointsLayer, HostScreen));
    }
}