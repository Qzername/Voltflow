using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voltflow.Models;

namespace Voltflow.ViewModels.Pages.Statistics;

public class StatisticsViewModel(IScreen screen) : StatisticsPanelBase(screen)
{
	//grid
	[Reactive] public List<GridElement> Elements { get; set; } = [];

	protected override void GenerateEnergyUsedData()
	{
		Dictionary<Car, float> total = new();

		foreach (var t in Transactions.Values)
		{
			if(t.CarId is null)
                continue;

            //TODO: optimize this
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
		Dictionary<Car, float> total = new();

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
		List<GridElement> elementsTemp = new();

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

	public void NavigateAdvanced() => HostScreen.Router.NavigateAndReset.Execute(new AdvancedStatisticsViewModel(HostScreen));

	public class GridElement
	{
		public string? CarName { get; set; }
		public double EnergyConsumed { get; set; }
		public double Cost { get; set; }
	}
}
