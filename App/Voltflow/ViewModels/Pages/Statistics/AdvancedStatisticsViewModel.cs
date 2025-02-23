using LiveChartsCore.SkiaSharpView;
using ReactiveUI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Voltflow.Models;
using System.Linq;
using ReactiveUI.Fody.Helpers;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.VisualElements;
using Voltflow.Services;
using Avalonia.Platform.Storage;
using System.IO;

namespace Voltflow.ViewModels.Pages.Statistics;

public class AdvancedStatisticsViewModel : ViewModelBase
{
    HttpClient _httpClient;
    DialogService _dialogService;

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
    [Reactive] public List<TransactionGridElement> TransactionsGridData { get; set; }
    [Reactive] public List<StationGridElement> StationGridData { get; set; }

    public AdvancedStatisticsViewModel(IScreen screen) : base(screen)
    {
        _httpClient = GetService<HttpClient>();
        _dialogService = GetService<DialogService>();

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
        //transactions
        List<TransactionGridElement> elementsTemp = new();

        foreach (var t in transactions)
        {
            elementsTemp.Add(new TransactionGridElement
            {
                StationID = stations.Single(station => station.Id == t.ChargingStationId).Id,
                EnergyConsumed = t.EnergyConsumed,
                Cost = t.Cost
            });
        }

        TransactionsGridData = new List<TransactionGridElement>(elementsTemp);
    
        //stations
        List<StationGridElement> stationGridElements = new List<StationGridElement>();

        foreach(var s in stations)
        {
            stationGridElements.Add(new StationGridElement()
            {
                StationID = s.Id,
                Longitutde = s.Longitude,
                Latitude = s.Latitude,
            });
        }

        StationGridData = new List<StationGridElement>(stationGridElements);
    }

    public void GenerateTransactionsCsv()
    {
        string csv = "StationID,EnergyConsumed,Cost\n";

        foreach (var t in transactions)
            csv += $"{t.ChargingStationId},{t.EnergyConsumed},{t.Cost}\n";

        SaveCsv(csv);
    }

    public void GenerateStationsCsv()
    {
        string csv = "StationID,Latitude,Longitutde\n";

        foreach (var s in stations)
            csv += $"{s.Id},{s.Latitude},{s.Longitude}\n";

        SaveCsv(csv);
    }

    public async void SaveCsv(string csv)
    {
        var directory = await _dialogService.OpenDirectoryDialog(new FolderPickerOpenOptions()
        {
            AllowMultiple = false,
        });

        File.WriteAllText(directory + "output.csv", csv);
    }

    public class TransactionGridElement
    {
        public int StationID { get; set; }
        public double EnergyConsumed { get; set; }
        public double Cost { get; set; }
    }

    public class StationGridElement
    {
        public int StationID { get; set; }
        public double Latitude { get; set; }
        public double Longitutde { get; set; }
    }
}
