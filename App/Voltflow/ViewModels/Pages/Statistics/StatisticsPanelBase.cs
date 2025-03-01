using Avalonia;
using Avalonia.Controls.Notifications;
using DynamicData;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.VisualBasic;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Ursa.Controls;
using Voltflow.Models;
using Voltflow.Services;

namespace Voltflow.ViewModels.Pages.Statistics;

/// <summary>
/// General class for two statistics panels since they are very similar.
/// </summary>
public abstract class StatisticsPanelBase : ViewModelBase
{
	[Reactive] public string Title { get; set; } = "Cost";

	public WindowToastManager? ToastManager;
	public Visual? Parent;

	// PieChart handling
	public ObservableCollection<ISeries> PieData { get; set; } = [];
	public bool Mode
	{
		set
		{
			PieData.Clear();
			PieData.AddRange(value ? EnergyData : CostData);
			Title = value ? "Energy" : "Cost";
		}
	}

	private readonly DialogService _dialogService;
	protected readonly HttpClient HttpClient;

	protected Dictionary<int, Transaction> Transactions = [];
	protected Dictionary<int, ChargingStation> Stations = [];
	protected Dictionary<int, Car> Cars = [];

	protected List<PieSeries<float>> EnergyData = [];
	protected List<PieSeries<float>> CostData = [];

	protected StatisticsPanelBase(bool loadAllTransactions, IScreen screen) : base(screen)
	{
		_dialogService = GetService<DialogService>();
		HttpClient = GetService<HttpClient>();

		GetData(loadAllTransactions);
	}

	private async void GetData(bool loadAllTransactions)
	{
		var monthAgo = DateTime.Now.AddMonths(-1);
		var monthAgoString = monthAgo.ToString("o");

        // --- prepare data for generation ---
        //Transactions
        var request = await HttpClient.GetAsync($"/api/Transactions{(loadAllTransactions?"/all":string.Empty)}?since"+monthAgoString);

		if (request.StatusCode != HttpStatusCode.OK)
			return;

		var json = await request.Content.ReadAsStringAsync();

		var tempTransactions = JsonConverter.Deserialize<Transaction[]>(json);
		Transactions = tempTransactions!.ToDictionary(tempTransactions => tempTransactions.Id);

		//Stations
		request = await HttpClient.GetAsync("/api/ChargingStations");

		if (request.StatusCode != HttpStatusCode.OK)
			return;

		json = await request.Content.ReadAsStringAsync();

		var tempStations = JsonConverter.Deserialize<ChargingStation[]>(json);
		Stations = tempStations!.ToDictionary(tempStations => tempStations.Id);

		//Cars
		request = await HttpClient.GetAsync("/api/Cars");

		if (request.StatusCode != HttpStatusCode.OK)
			return;

		json = await request.Content.ReadAsStringAsync();

		var tempCars = JsonConverter.Deserialize<Car[]>(json);
		Cars = tempCars!.ToDictionary(tempCars => tempCars.Id);

		// --- pie chart data generation ---
		GenerateCostData();
		GenerateEnergyUsedData();
		GenerateGridData();

		Mode = false;
	}

	protected abstract void GenerateEnergyUsedData();
	protected abstract void GenerateCostData();
	protected abstract void GenerateGridData();

	protected static List<PieSeries<float>> ConstructPieChartSeries(Dictionary<ChargingStation, float> total)
	{
		List<PieSeries<float>> dataTemp = [];

		foreach (var t in total)
		{
			dataTemp.Add(new PieSeries<float>
			{
				Values = [t.Value],
			});
		}

		return dataTemp;
	}

	protected static List<PieSeries<float>> ConstructPieChartSeries(Dictionary<Car, float> total)
	{
		List<PieSeries<float>> dataTemp = [];

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

		return dataTemp;
	}

	protected async Task SaveCsv(string csv)
	{
		var results = await _dialogService.SaveFileDialog(Parent, csv);

		ToastManager?.Show(
			new Toast(results ? "Successfully saved the file!" : "Couldn't save the file!"),
			showIcon: true,
			showClose: false,
			type: results ? NotificationType.Success : NotificationType.Error,
			classes: ["Light"]);
	}
}
