using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voltflow.Models;

namespace Voltflow.ViewModels.Pages.Statistics;

public class StatisticsViewModel : StatisticsPanelBase
{
	//grid
	[Reactive] public List<GridElement> Elements { get; set; } = [];

	public StatisticsViewModel(IScreen screen) : base(screen)
	{
	}

    protected override void GenerateEnergyUsedData()
	{
		Dictionary<Car, float> total = new();

		foreach (var t in transactions.Values)
		{
			if(t.CarId is null)
                continue;

            //TODO: optimize this
            var car = cars[t.CarId.Value];

			if (total.ContainsKey(car))
				total[car] += (float)t.EnergyConsumed;
			else
				total[car] = (float)t.EnergyConsumed;
		}

		energyData = ConstructPieChartSeries(total);
	}

    protected override void GenerateCostData()
	{
		Dictionary<Car, float> total = new();

		foreach (var t in transactions.Values)
		{
			if (t.CarId is null)
				continue;

			var car = cars[t.CarId.Value];

            if (total.ContainsKey(car))
				total[car] += (float)t.Cost;
			else
				total[car] = (float)t.Cost;
		}

		costData = ConstructPieChartSeries(total);
	}

    protected override void GenerateGridData()
	{
		List<GridElement> elementsTemp = new();

		foreach (var t in transactions.Values)
		{
			if (t.CarId is null)
				continue;

			elementsTemp.Add(new GridElement
			{
				CarName = cars[t.CarId.Value].Name,
				EnergyConsumed = t.EnergyConsumed,
				Cost = t.Cost
			});
		}

		Elements = new List<GridElement>(elementsTemp);
	}

	public async Task GenerateCsv()
	{
		string csv = "Station Id,Energy Consumed,Cost\n";

		foreach (var t in transactions.Values)
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
