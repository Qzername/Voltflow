using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using ReactiveUI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Voltflow.Models;
using System.Linq;
using SkiaSharp;
using ReactiveUI.Fody.Helpers;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.VisualElements;

namespace Voltflow.ViewModels.Pages.Statistics;

public class AdvancedStatisticsViewModel : ViewModelBase
{
    HttpClient _httpClient;

    public bool Mode
    {
        set
        {
            if (value)
                EnergyUsedPieMode();
            else
                CostPieMode();
        }
    }

    [Reactive] public IEnumerable<ISeries> PieData { get; set; }
    public LabelVisual Title { get; set; } =
    new LabelVisual
    {
        Text = "My chart title",
        TextSize = 25,
        Padding = new LiveChartsCore.Drawing.Padding(15)
    };

    Transaction[] transactions;
    ChargingStation[] stations;

    //grid
    [Reactive] public List<GridElement> Elements { get; set; }

    public AdvancedStatisticsViewModel(IScreen screen) : base(screen)
    {
        _httpClient = GetService<HttpClient>();

        GetData();
    }

    async void GetData()
    {
        //transactions
        var response = await _httpClient.GetAsync("/api/Transactions/all");
        Debug.WriteLine(response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();

        transactions = JsonConverter.Deserialize<Transaction[]>(json);

        //stations
        response = await _httpClient.GetAsync("/api/ChargingStations");
        Debug.WriteLine(response.StatusCode);
        json = await response.Content.ReadAsStringAsync();

        stations = JsonConverter.Deserialize<ChargingStation[]>(json);

        CostPieMode();
        GenerateGridData();
    }

    void EnergyUsedPieMode()
    {
        Dictionary<ChargingStation, float> total = new();

        foreach (var t in transactions)
        {
            //TODO: optimize this
            var station = stations.Single(station => station.Id == t.ChargingStationId);

            if (total.ContainsKey(station))
                total[station] += (float)t.EnergyConsumed;
            else
                total[station] = (float)t.EnergyConsumed;
        }

        ConstructPieChartSeries(total);
    }

    void CostPieMode()
    {
        Dictionary<ChargingStation, float> total = new();

        foreach (var t in transactions)
        {
            //TODO: optimize this
            var station = stations.Single(station => station.Id == t.ChargingStationId);

            if (total.ContainsKey(station))
                total[station] += (float)t.Cost;
            else
                total[station] = (float)t.Cost;
        }

        ConstructPieChartSeries(total);
    }

    void ConstructPieChartSeries(Dictionary<ChargingStation, float> total)
    {
        List<PieSeries<float>> dataTemp = new();

        foreach (var t in total)
        {
            dataTemp.Add(new PieSeries<float>
            {
                Values = [t.Value],
            });
        }

        PieData = dataTemp;
    }

    void GenerateGridData()
    {
        List<GridElement> elementsTemp = new();

        foreach (var t in transactions)
        {
            elementsTemp.Add(new GridElement
            {
                StationID = stations.Single(station => station.Id == t.ChargingStationId).Id,
                EnergyConsumed = t.EnergyConsumed,
                Cost = t.Cost
            });
        }

        Elements = new List<GridElement>(elementsTemp);
    }

    public class GridElement
    {
        public int StationID { get; set; }
        public double EnergyConsumed { get; set; }
        public double Cost { get; set; }
    }
}
