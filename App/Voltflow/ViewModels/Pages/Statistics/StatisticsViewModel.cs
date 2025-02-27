﻿using Avalonia;
using Avalonia.Controls.Notifications;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Ursa.Controls;
using Voltflow.Models;
using Voltflow.Services;

namespace Voltflow.ViewModels.Pages.Statistics;

public class StatisticsViewModel : ViewModelBase
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
				EnergyUsedPieMode();
			else
				CostPieMode();
		}
	}

	[Reactive] public IEnumerable<ISeries> PieData { get; set; } = [];

	private Transaction[] _transactions = [];
	private Car[] _cars = [];

	//grid
	[Reactive] public List<GridElement> Elements { get; set; } = [];

	public StatisticsViewModel(IScreen screen) : base(screen)
	{
		_httpClient = GetService<HttpClient>();
		_dialogService = GetService<DialogService>();

		GetData();
	}

	private async void GetData()
	{
		//transactions
		var request = await _httpClient.GetAsync("/api/Transactions");

		if (request.StatusCode != HttpStatusCode.OK)
			return;

		var json = await request.Content.ReadAsStringAsync();

		_transactions = JsonConverter.Deserialize<Transaction[]>(json);

		//cars
		request = await _httpClient.GetAsync("/api/Cars");

		if (request.StatusCode != HttpStatusCode.OK)
			return;

		json = await request.Content.ReadAsStringAsync();

		_cars = JsonConverter.Deserialize<Car[]>(json);

		CostPieMode();
		GenerateGridData();
	}

	private void EnergyUsedPieMode()
	{
		Dictionary<Car, float> total = new();

		foreach (var t in _transactions)
		{
			//TODO: optimize this
			var car = _cars.Single(cars => cars.Id == t.CarId);

			if (total.ContainsKey(car))
				total[car] += (float)t.EnergyConsumed;
			else
				total[car] = (float)t.EnergyConsumed;
		}

		ConstructPieChartSeries(total);
	}

	private void CostPieMode()
	{
		Dictionary<Car, float> total = new();

		foreach (var t in _transactions)
		{
			if (t.CarId is null)
				continue;

			//TODO: optimize this
			var car = _cars.Single(cars => cars.Id == t.CarId);

			if (total.ContainsKey(car))
				total[car] += (float)t.Cost;
			else
				total[car] = (float)t.Cost;
		}

		ConstructPieChartSeries(total);
	}

	private void ConstructPieChartSeries(Dictionary<Car, float> total)
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

	private void GenerateGridData()
	{
		List<GridElement> elementsTemp = new();

		foreach (var t in _transactions)
		{
			if (t.CarId is null)
				continue;

			elementsTemp.Add(new GridElement
			{
				CarName = _cars.Single(cars => cars.Id == t.CarId).Name,
				EnergyConsumed = t.EnergyConsumed,
				Cost = t.Cost
			});
		}

		Elements = new List<GridElement>(elementsTemp);
	}

	public async Task GenerateCsv()
	{
		string csv = "Station Id,Energy Consumed,Cost\n";

		foreach (var t in _transactions)
			csv += $"{t.ChargingStationId},{t.EnergyConsumed},{t.Cost}\n";

		var results = await _dialogService.SaveFileDialog(Parent, csv);

		ToastManager?.Show(
			new Toast(results ? "Successfully saved the file!" : "Couldn't save the file!"),
			showIcon: true,
			showClose: false,
			type: results ? NotificationType.Success : NotificationType.Error,
			classes: ["Light"]);
	}

	public void NavigateAdvanced() => HostScreen.Router.NavigateAndReset.Execute(new AdvancedStatisticsViewModel(HostScreen));

	public class GridElement
	{
		public string? CarName { get; set; }
		public double EnergyConsumed { get; set; }
		public double Cost { get; set; }
	}
}
