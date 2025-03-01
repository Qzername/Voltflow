using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voltflow.Models;

namespace Voltflow.ViewModels.Pages.Statistics;

public class StatisticsViewModel(IScreen screen, bool isAdmin) : StatisticsPanelBase(false, screen)
{
	[Reactive] public List<GridElement> Elements { get; set; } = [];
	[Reactive] public bool IsAdmin { get; set; } = isAdmin;

	protected override void GenerateEnergyUsedData()
	{
		Dictionary<Car, float> total = [];

		foreach (var t in Transactions.Values)
		{
			if (t.CarId is null)
				continue;

			var car = Cars[t.CarId.Value];

			if (total.ContainsKey(car))
				total[car] += (float)t.EnergyConsumed;
			else
				total[car] = (float)t.EnergyConsumed;
		}

		EnergyData = ConstructPieChartSeries(total);
	}

	protected override void GenerateCostData()
	{
		Dictionary<Car, float> total = [];

		foreach (var t in Transactions.Values)
		{
			if (t.CarId is null)
				continue;

			var car = Cars[t.CarId.Value];

			if (total.ContainsKey(car))
				total[car] += (float)t.Cost;
			else
				total[car] = (float)t.Cost;
		}

		CostData = ConstructPieChartSeries(total);
	}

	protected override void GenerateGridData()
	{
		List<GridElement> elementsTemp = [];

		foreach (var t in Transactions.Values)
		{
			if (t.CarId is null)
				continue;

			elementsTemp.Add(new GridElement
			{
				CarName = Cars[t.CarId.Value].Name,
				EnergyConsumed = t.EnergyConsumed,
				Cost = t.Cost
			});
		}

		Elements = new List<GridElement>(elementsTemp);
	}

	public async Task GenerateCsv()
	{
		string csv = "Station Id,Energy Consumed,Cost\n";

		foreach (var t in Transactions.Values)
			csv += $"{t.ChargingStationId},{t.EnergyConsumed},{t.Cost}\n";

		await SaveCsv(csv);
	}

	public void NavigateToAdvanced() => HostScreen.Router.Navigate.Execute(new AdvancedStatisticsViewModel(HostScreen));
	public void NavigateToStatisticsData() => HostScreen.Router.Navigate.Execute(new StatisticsDataViewModel(HostScreen, elements: Elements));
}
