using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using Voltflow.Models;

namespace Voltflow.ViewModels.Pages.Statistics;

public class AdvancedStatisticsViewModel(IScreen screen) : StatisticsPanelBase(true, screen)
{
	public ReactiveCommand<Unit, IRoutableViewModel> GoBack => HostScreen.Router.NavigateBack;

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
		Dictionary<int, TempStationData> tempStationData = [];

        //transactions
        List<TransactionGridElement> elementsTemp = [];

		foreach (var t in Transactions.Values)
		{
			if(tempStationData.ContainsKey(t.ChargingStationId))
            {
				var temp = tempStationData[t.ChargingStationId];

				temp.NumberOfCharges++;
                temp.LastCharge = t.EndDate > temp.LastCharge ? t.EndDate : temp.LastCharge;

                tempStationData[t.ChargingStationId] = temp;
            }
            else
            {
                tempStationData[t.ChargingStationId] = new TempStationData
                {
                    NumberOfCharges = 1,
                    LastCharge = t.EndDate
                };
            }

            elementsTemp.Add(new TransactionGridElement
            {
                CarName = t.CarId is null ? "null" : Cars[t.CarId.Value].Name,
                StationId = Stations[t.ChargingStationId].Id,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
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
                LastCharge = tempStationData.ContainsKey(s.Id) ? tempStationData[s.Id].LastCharge : null,
                NumberOfChargers = tempStationData.ContainsKey(s.Id) ? tempStationData[s.Id].NumberOfCharges : 0
            });
		}

		StationGridData = new List<StationGridElement>(stationGridElements);
	}

	#region CSV handling
	public async Task GenerateTransactionsCsv()
	{
        string csv = "Car Name,Start Date,End date,Station Id,Energy Consumed,Cost\n";

        foreach (var t in Transactions.Values)
            csv += $"{Cars[t.CarId.Value].Name},{t.StartDate.ToString()},{t.EndDate.ToString()},{t.ChargingStationId},{t.EnergyConsumed},{t.Cost}\n";

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

	public void NavigateToStationsData() => HostScreen.Router.Navigate.Execute(new StatisticsGridDataViewModel(HostScreen, stations: StationGridData));
	public void NavigateToTransactionsData() => HostScreen.Router.Navigate.Execute(new StatisticsGridDataViewModel(HostScreen, transactions: TransactionsGridData));


	struct TempStationData
	{
		public int NumberOfCharges;
        public DateTime LastCharge;
    }
}
