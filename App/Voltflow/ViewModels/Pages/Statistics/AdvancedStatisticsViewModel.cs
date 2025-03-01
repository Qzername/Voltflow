using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voltflow.Models;

namespace Voltflow.ViewModels.Pages.Statistics;

public class AdvancedStatisticsViewModel(IScreen screen) : StatisticsPanelBase(true, screen)
{
	//grid
	[Reactive] public List<TransactionGridElement> TransactionsGridData { get; set; } = [];
	[Reactive] public List<StationGridElement> StationGridData { get; set; } = [];

	#region Chart data generation
	protected override void GenerateEnergyUsedData()
	{
		Dictionary<ChargingStation, float> total = [];

		foreach (var t in Transactions.Values)
		{
			var station = Stations[t.ChargingStationId];

			if (total.ContainsKey(station))
				total[station] += (float)t.EnergyConsumed;
			else
				total[station] = (float)t.EnergyConsumed;
		}

		EnergyData = ConstructPieChartSeries(total);
	}

	protected override void GenerateCostData()
	{
		Dictionary<ChargingStation, float> total = [];

		foreach (var t in Transactions.Values)
		{
			var station = Stations[t.ChargingStationId];

			if (total.ContainsKey(station))
				total[station] += (float)t.Cost;
			else
				total[station] = (float)t.Cost;
		}

		CostData = ConstructPieChartSeries(total);
	}
	#endregion

	protected override void GenerateGridData()
	{
		//transactions
		List<TransactionGridElement> elementsTemp = [];

		foreach (var t in Transactions.Values)
		{
			elementsTemp.Add(new TransactionGridElement
			{
				StationId = Stations[t.ChargingStationId].Id,
				EnergyConsumed = t.EnergyConsumed,
				Cost = t.Cost
			});
		}

		TransactionsGridData = new List<TransactionGridElement>(elementsTemp);

		//stations
		List<StationGridElement> stationGridElements = [];

		foreach (var s in Stations.Values)
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

	#region CSV handling
	public async Task GenerateTransactionsCsv()
	{
		string csv = "Station Id,Energy Consumed,Cost\n";

		foreach (var t in Transactions.Values)
			csv += $"{t.ChargingStationId},{t.EnergyConsumed},{t.Cost}\n";

		await SaveCsv(csv);
	}

	public async Task GenerateStationsCsv()
	{
		string csv = "Station Id,Latitude,Longitude\n";

		foreach (var s in Stations.Values)
			csv += $"{s.Id},{s.Latitude},{s.Longitude}\n";

		await SaveCsv(csv);
	}
	#endregion

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
