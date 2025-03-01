using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Mapsui;
using Mapsui.Layers;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Voltflow.Models;
using Voltflow.Models.Statistics;
using Voltflow.ViewModels.Pages.Map.SidePanels;

namespace Voltflow.ViewModels.Pages.Map;

public class StationStatisticsViewModel : ViewModelBase
{
    [Reactive] public IEnumerable<ISeries> WeekUsage { get; set; } = [];
    [Reactive] public IEnumerable<ISeries> PeekHours { get; set; } = [];

    [Reactive] public OpeningHours Hours { get; set; }


    private readonly ChargingStation _chargingStation;
    ChargingStationOpeningHours _openingHours;
    private readonly HttpClient _httpClient;

    public StationStatisticsViewModel(ChargingStation currentStation, IScreen screen) : base(screen)
    {
        _httpClient = GetService<HttpClient>();
        _chargingStation = currentStation;

        GetStatistics();
        GetOpeningHours();
    }

    async void GetStatistics()
    {
        // --- week usage ---
        var request = await _httpClient.GetAsync("/api/Statistics/ChargingStations/weekUsage?stationId=" + _chargingStation.Id);
        Debug.WriteLine(request.StatusCode + " - ignore if BadRequest");

        //new station, no data
        if (request.StatusCode == HttpStatusCode.BadRequest)
        {
            WeekUsage = new List<ISeries>()
            {
                new ColumnSeries<int>
                {
                    Values = [0, 0, 0, 0, 0, 0, 0],
                }
            };

            PeekHours = new List<ISeries>()
            {
                new ColumnSeries<int>
                {
                    Values = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,],
                }
            };

            return;
        }

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
        Debug.WriteLine(request.StatusCode);
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

    async void GetOpeningHours()
    {
        var request = await _httpClient.GetAsync("/api/ChargingStations/OpeningHours?stationId=" + _chargingStation.Id);
        Debug.WriteLine(request.StatusCode);

        var json = await request.Content.ReadAsStringAsync();
        var temp = JsonConverter.Deserialize<ChargingStationOpeningHours>(json)!;

        _openingHours = temp;

        Hours = new OpeningHours()
        {
            Monday = [temp.Monday[0].ToString(), temp.Monday[1].ToString()],
            Tuesday = [temp.Tuesday[0].ToString(), temp.Tuesday[1].ToString()],
            Wednesday = [temp.Wednesday[0].ToString(), temp.Wednesday[1].ToString()],
            Thursday = [temp.Thursday[0].ToString(), temp.Thursday[1].ToString()],
            Friday = [temp.Friday[0].ToString(), temp.Friday[1].ToString()],
            Saturday = [temp.Saturday[0].ToString(), temp.Saturday[1].ToString()],
            Sunday = [temp.Sunday[0].ToString(), temp.Sunday[1].ToString()],
        };
    }

    public void NavigateToModifyHours()
    {
        HostScreen.Router.Navigate.Execute(new ModifyHoursViewModel(_chargingStation, _openingHours, HostScreen));
    }

    public struct OpeningHours
    {
        public string[] Monday { get; set; }
        public string[] Tuesday { get; set; }
        public string[] Wednesday { get; set; }
        public string[] Thursday { get; set; }
        public string[] Friday { get; set; }
        public string[] Saturday { get; set; }
        public string[] Sunday { get; set; }
    }
}