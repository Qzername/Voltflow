using Avalonia.Collections;
using Avalonia.SimplePreferences;
using Microsoft.AspNetCore.SignalR.Client;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Voltflow.Models;
using Voltflow.ViewModels.Pages.Map;
using static System.Collections.Specialized.BitVector32;

namespace Voltflow.ViewModels.Pages.Charging;

public class ChargingViewModel : ViewModelBase
{
    [Reactive] public ChargingStation CurrentStation { get; set; }

    AvaloniaList<Car> Cars { get; set; } = new();
    [Reactive] public int PickedIndex { get; set; } 

    [Reactive] public bool DeclaredAmount { get; set; }
	[Reactive] public int Amount { get; set; }
    
    [Reactive] public string Time { get; set; }
	[Reactive] public double TotalCharge { get; set; }
	[Reactive] public double TotalCost { get; set; }

	HubConnection connection;
    Timer dataUpdate;
    DateTime startTime;

    public ChargingViewModel(ChargingStation chargingStation, IScreen screen) : base(screen)
    {
        CurrentStation = chargingStation;

        dataUpdate = new Timer();
        dataUpdate.Interval = 1000;
        dataUpdate.Elapsed += (sender, e) => UpdateData();

        GetCarsForCombobox();
    }

    async void GetCarsForCombobox()
    {
        //get cars for combobox
        var httpClient = GetService<HttpClient>();
        var response = await httpClient.GetAsync("/api/Cars");
        var jsonString = await response.Content.ReadAsStringAsync();
        Debug.WriteLine(response.StatusCode);

        var carsObjs = JsonConverter.Deserialize<Car[]>(jsonString);
        Cars.AddRange(carsObjs);

        PickedIndex = 0;
    }

	public async Task Start()
	{
        var token = Preferences.Get<string?>("token", null);
        var carId = Cars[PickedIndex].Id;

        connection = new HubConnectionBuilder().WithUrl($"https://voltflow-api.heapy.xyz/charginghub?carId={carId}&stationId={CurrentStation.Id}", (options) =>
        {
            options.AccessTokenProvider = () => Task.FromResult(token);
        }).Build();

        Debug.WriteLine("Connecting...");
        await connection.StartAsync();
        Debug.WriteLine(connection.State.ToString());
        
        startTime = DateTime.UtcNow;
        dataUpdate.Enabled = true;
    }

    void UpdateData()
    {
        var car = Cars[PickedIndex];
        var chargingRate = CurrentStation.MaxChargeRate > car.ChargingRate ? car.ChargingRate : CurrentStation.MaxChargeRate;

        var time = DateTime.UtcNow - startTime;
        Time = time.ToString("hh\\:mm\\:ss");
        TotalCharge = chargingRate * time.TotalSeconds/1000; //per kwh
        TotalCost = TotalCharge * CurrentStation.Cost;

        if (DeclaredAmount && TotalCharge >= Amount)
            _ = Stop();
    }

    public async Task Stop()
    {
        await connection.StopAsync();
        Debug.WriteLine(connection.State.ToString());

        dataUpdate.Enabled = false;
    }

    //navigation
    public void NavigateHome() => HostScreen.Router.NavigateAndReset.Execute(new MapViewModel(HostScreen));
}