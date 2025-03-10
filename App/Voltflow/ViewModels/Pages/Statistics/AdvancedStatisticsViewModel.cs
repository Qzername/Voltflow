﻿using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Voltflow.Models;

namespace Voltflow.ViewModels.Pages.Statistics;

public class AdvancedStatisticsViewModel : StatisticsPanelBase
{
    private readonly HttpClient _httpClient;

    //grid
    [Reactive] public List<TransactionGridElement> TransactionsGridData { get; set; } = [];
    [Reactive] public List<StationGridElement> StationGridData { get; set; } = [];

    public AdvancedStatisticsViewModel(IScreen screen) : base(true, screen)
    {
        _httpClient = GetService<HttpClient>();
    }

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

    protected override async void GenerateGridData()
    {
        Dictionary<int, TempStationData> tempStationData = [];

        //transactions
        List<TransactionGridElement> elementsTemp = [];

        foreach (var t in Transactions.Values)
        {
            if (tempStationData.ContainsKey(t.ChargingStationId))
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
                StationId = Stations[t.ChargingStationId].Id,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                EnergyConsumed = t.EnergyConsumed,
                Cost = t.Cost
            });
        }

        TransactionsGridData = new List<TransactionGridElement>(elementsTemp);

        //service history 
        var weekAgo = DateTime.Now.AddDays(-7);
        var weekAgoString = weekAgo.ToString("o");

        var request = await _httpClient.GetAsync("api/ChargingStations/ServiceHistory?since" + weekAgoString);

        var json = await request.Content.ReadAsStringAsync();
        var serviceHistoryTemp = JsonConverter.Deserialize<ChargingStationsServiceHistory[]>(json);

        Dictionary<int, ChargingStationsServiceHistory[]> serviceHistory = [];

        if (serviceHistoryTemp is not null)
            serviceHistory = serviceHistoryTemp.GroupBy(x=>x.StationId).ToDictionary(x => x.Key, x => serviceHistoryTemp.Where(y => y.StationId == x.Key).ToArray());

        //stations
        List<StationGridElement> stationGridElements = [];

        foreach (var s in Stations.Values)
        {
            //check for possible warnings
            string warning = "";

            if (serviceHistory.ContainsKey(s.Id))
                Debug.WriteLine(serviceHistory[s.Id].Length);

            if (serviceHistory.ContainsKey(s.Id) && serviceHistory[s.Id].Length > 5)
                warning = serviceHistory[s.Id].Length + " services in last week, ";

            if (!tempStationData.ContainsKey(s.Id))
                warning += "no charges in last week";

            stationGridElements.Add(new StationGridElement()
            {
                StationId = s.Id,
                Warning = warning,
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
        string csv = "Id;Start Date;End date;Station Id;Energy Consumed;Cost\n";

        foreach (var t in Transactions.Values)
            csv += $"{t.Id},{t.StartDate.ToString()},{t.EndDate.ToString()},{t.ChargingStationId},{t.EnergyConsumed},{t.Cost}\n";

        await SaveCsv(csv);
    }

    public async Task GenerateStationsCsv()
    {
        string csv = "Station Id;Warnings;Latitude;Longitude;Last Charge;Number Of Charges\n";

        foreach (var s in StationGridData)
            csv += $"{s.StationId};{s.Warning};{s.Latitude};{s.Longitude};{s.LastCharge};{s.NumberOfChargers}\n";

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
