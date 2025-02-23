using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.VisualElements;
using ReactiveUI;
using System.Collections.Generic;
using System.Net.Http;
using System.Diagnostics;
using Voltflow.Models;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Linq;
using ReactiveUI.Fody.Helpers;
using Voltflow.ViewModels.Pages.Map;
using Voltflow.Services;
using Avalonia.Platform.Storage;
using System.IO;

namespace Voltflow.ViewModels.Pages.Statistics;

public class StatisticsViewModel : ViewModelBase
{
    HttpClient _httpClient;
    DialogService _dialogService;

    public bool Mode
    {
        set
        {
            if(value)
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
    Car[] cars;

    //grid
    [Reactive] public List<GridElement> Elements { get; set; }

    public StatisticsViewModel(IScreen screen) : base(screen)
    {
        _httpClient = GetService<HttpClient>();
        _dialogService = GetService<DialogService>();

        GetData();
    }

    async void GetData()
    {
        //transactions
        var response = await _httpClient.GetAsync("/api/Transactions");
        Debug.WriteLine(response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();

        transactions = JsonConverter.Deserialize<Transaction[]>(json);

        //cars
        response = await _httpClient.GetAsync("/api/Cars");
        Debug.WriteLine(response.StatusCode);
        json = await response.Content.ReadAsStringAsync();

        cars = JsonConverter.Deserialize<Car[]>(json);

        CostPieMode();
        GenerateGridData();
    }

    void EnergyUsedPieMode()
    {
        Dictionary<Car, float> total = new();

        foreach(var t in transactions)
        {
            //TODO: optimize this
            var car = cars.Single(cars => cars.Id == t.CarId);

            if (total.ContainsKey(car))
                total[car] += (float)t.EnergyConsumed;
            else
                total[car] = (float)t.EnergyConsumed;
        }

        ConstructPieChartSeries(total);
    }

    void CostPieMode()
    {
        Dictionary<Car, float> total = new();

        foreach (var t in transactions)
        {
            //TODO: optimize this
            var car = cars.Single(cars => cars.Id == t.CarId);

            if (total.ContainsKey(car))
                total[car] += (float)t.Cost;
            else
                total[car] = (float)t.Cost;
        }

        ConstructPieChartSeries(total);
    }

    void ConstructPieChartSeries(Dictionary<Car, float> total)
    {
        List<PieSeries<float>> dataTemp = new();

        foreach (var t in total)
        {
            dataTemp.Add(new PieSeries<float>
            {
                Name = t.Key.Name,
                Values = [t.Value],
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsSize = 15,
                DataLabelsPaint = new SolidColorPaint(new SKColor(255, 255, 255)),
                DataLabelsFormatter = point => t.Key.Name
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
                CarName = cars.Single(cars => cars.Id == t.CarId).Name,
                EnergyConsumed = t.EnergyConsumed,
                Cost = t.Cost
            });
        }

        Elements = new List<GridElement>(elementsTemp);
    }

    public async void GenerateCsv()
    {
        string csv = "StationID,EnergyConsumed,Cost\n";

        foreach (var t in transactions)
            csv += $"{t.ChargingStationId},{t.EnergyConsumed},{t.Cost}\n";

        var directory = await _dialogService.OpenDirectoryDialog(new FolderPickerOpenOptions()
        {
            AllowMultiple = false,
        });

        File.WriteAllText(directory + "output.csv", csv);
    }

    public void NavigateAdvanced() => HostScreen.Router.NavigateAndReset.Execute(new AdvancedStatisticsViewModel(HostScreen));
    public void NavigateHome() => HostScreen.Router.NavigateAndReset.Execute(new MapViewModel(HostScreen));

    public class GridElement
    {
        public string CarName { get; set; }
        public double EnergyConsumed { get; set; }
        public double Cost { get; set; }
    }
}
