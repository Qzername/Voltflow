using Avalonia;
using Avalonia.Controls.Notifications;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Ursa.Controls;
using Voltflow.Models;
using Voltflow.Services;

namespace Voltflow.ViewModels.Pages.Statistics;

public class AdvancedStatisticsViewModel : ViewModelBase
{
	public WindowToastManager? ToastManager;
	public Visual? Parent;

	private readonly HttpClient _httpClient;
	private readonly DialogService _dialogService;

	public bool Mode
	{
		set
		{
			if (value)
				PieData = energyData;
            else
				PieData = costData;
        }
	}

	[Reactive] public IEnumerable<ISeries> PieData { get; set; } = [];

	private Transaction[] _transactions = [];
	private ChargingStation[] _stations = [];

	List<PieSeries<float>> energyData;
	List<PieSeries<float>> costData;

	//grid
	[Reactive] public List<TransactionGridElement> TransactionsGridData { get; set; } = [];
	[Reactive] public List<StationGridElement> StationGridData { get; set; } = [];

	public AdvancedStatisticsViewModel(IScreen screen) : base(screen)
	{
		_httpClient = GetService<HttpClient>();
		_dialogService = GetService<DialogService>();

		energyData = new List<PieSeries<float>>();
        costData = new List<PieSeries<float>>();

        GetData();
	}

	private async void GetData()
	{
		//transactions
		var request = await _httpClient.GetAsync("/api/Transactions/all");

		if (request.StatusCode != HttpStatusCode.OK)
			return;

		var json = await request.Content.ReadAsStringAsync();

		_transactions = JsonConverter.Deserialize<Transaction[]>(json);

		//stations
		request = await _httpClient.GetAsync("/api/ChargingStations");

		if (request.StatusCode != HttpStatusCode.OK)
			return;

		json = await request.Content.ReadAsStringAsync();

		_stations = JsonConverter.Deserialize<ChargingStation[]>(json);

		CostPieMode();
		EnergyUsedPieMode();
	}

	private void EnergyUsedPieMode()
	{
		Dictionary<ChargingStation, float> total = new();

		foreach (var t in _transactions)
		{
			//TODO: optimize this
			var station = _stations.Single(station => station.Id == t.ChargingStationId);

			if (total.ContainsKey(station))
				total[station] += (float)t.EnergyConsumed;
			else
				total[station] = (float)t.EnergyConsumed;
		}

		ConstructPieChartSeries(total);
	}

	private void CostPieMode()
	{
		Dictionary<ChargingStation, float> total = new();

		foreach (var t in _transactions)
		{
			//TODO: optimize this
			var station = _stations.Single(station => station.Id == t.ChargingStationId);

			if (total.ContainsKey(station))
				total[station] += (float)t.Cost;
			else
				total[station] = (float)t.Cost;
		}

		ConstructPieChartSeries(total);
	}

	private List<PieSeries<float>> ConstructPieChartSeries(Dictionary<ChargingStation, float> total)
	{
		List<PieSeries<float>> dataTemp = new();

		foreach (var t in total)
		{
			dataTemp.Add(new PieSeries<float>
			{
				Values = [t.Value],
			});
		}

		return dataTemp;
	}

	private void GenerateGridData()
	{
		//transactions
		List<TransactionGridElement> elementsTemp = new();

		foreach (var t in _transactions)
		{
			elementsTemp.Add(new TransactionGridElement
			{
				StationId = _stations.Single(station => station.Id == t.ChargingStationId).Id,
				EnergyConsumed = t.EnergyConsumed,
				Cost = t.Cost
			});
		}

		TransactionsGridData = new List<TransactionGridElement>(elementsTemp);

		//stations
		List<StationGridElement> stationGridElements = new List<StationGridElement>();

		foreach (var s in _stations)
		{
			stationGridElements.Add(new StationGridElement()
			{
				StationId = s.Id,
				Longitude = s.Longitude,
				Latitude = s.Latitude,
			});
		}

		StationGridData = new List<StationGridElement>(stationGridElements);
	}

	public async Task GenerateTransactionsCsv()
	{
		string csv = "Station Id,Energy Consumed,Cost\n";

		foreach (var t in _transactions)
			csv += $"{t.ChargingStationId},{t.EnergyConsumed},{t.Cost}\n";

		await SaveCsv(csv);
	}

	public async Task GenerateStationsCsv()
	{
		string csv = "Station Id,Latitude,Longitude\n";

		foreach (var s in _stations)
			csv += $"{s.Id},{s.Latitude},{s.Longitude}\n";

		await SaveCsv(csv);
	}

	public async Task SaveCsv(string csv)
	{
		var results = await _dialogService.SaveFileDialog(Parent, csv);

		ToastManager?.Show(
			new Toast(results ? "Successfully saved the file!" : "Couldn't save the file!"),
			showIcon: true,
			showClose: false,
			type: results ? NotificationType.Success : NotificationType.Error,
			classes: ["Light"]);
	}

	public class TransactionGridElement
	{
		public int StationId { get; set; }
		public double EnergyConsumed { get; set; }
		public double Cost { get; set; }
	}

	public class StationGridElement
	{
		public int StationId { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
	}
}
