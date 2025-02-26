using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using Avalonia.SimplePreferences;
using Microsoft.AspNetCore.SignalR.Client;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Ursa.Controls;
using Voltflow.Models;
using Voltflow.ViewModels.Pages.Map;

namespace Voltflow.ViewModels.Pages.Charging;

public class ChargingViewModel : ViewModelBase
{
	public WindowToastManager? ToastManager;

	[Reactive] public ChargingStation CurrentStation { get; set; }

	AvaloniaList<Car> Cars { get; set; } = new();
	[Reactive] public int PickedIndex { get; set; } = 0;

	[Reactive] public bool DeclaredAmount { get; set; }
	[Reactive] public float Amount { get; set; }

	[Reactive] public string Time { get; set; } = "00:00:00";
	[Reactive] public double TotalCharge { get; set; }
	[Reactive] public double TotalCost { get; set; }

	// Started is false at first, after first start it will always be true.
	// It's used for displaying charging info.
	[Reactive] public bool Started { get; set; }
	[Reactive] public bool Finished { get; set; } = true;

	private HubConnection? _connection;
	private readonly Timer _dataUpdate = new();
	private DateTime _startTime;
	private readonly HttpClient _httpClient;

	public ChargingViewModel(ChargingStation chargingStation, IScreen screen) : base(screen)
	{
		_httpClient = GetService<HttpClient>();
		CurrentStation = chargingStation;

		_dataUpdate.Interval = 100;
		_dataUpdate.Elapsed += async (sender, e) => await UpdateData();

		GetCarsForCombobox();
	}

	private async void GetCarsForCombobox()
	{
		//get cars for combobox
		var request = await _httpClient.GetAsync("/api/Cars");
		var jsonString = await request.Content.ReadAsStringAsync();

		if (request.StatusCode != HttpStatusCode.OK)
			return;

		var carsObjs = JsonConverter.Deserialize<Car[]>(jsonString);
		Cars.AddRange(carsObjs);
	}

	public async Task Start()
	{
		if (Cars.Count == 0 || PickedIndex > Cars.Count)
		{
			ToastManager?.Show(
				new Toast("You must select a car!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Error,
				classes: ["Light"]);
			return;
		}

		if (DeclaredAmount && Amount <= 0)
		{
			ToastManager?.Show(
				new Toast("Declared amount must be above 0!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Error,
				classes: ["Light"]);
			return;
		}

		Time = "00:00:00";
		TotalCharge = 0;
		TotalCost = 0;

		Started = true;
		Finished = false;

		var token = await Preferences.GetAsync<string?>("token", null);
		var carId = Cars[PickedIndex].Id;

		_connection = new HubConnectionBuilder()
			.WithUrl($"https://voltflow-api.heapy.xyz/charginghub?carId={carId}&stationId={CurrentStation.Id}",
				(options) => { options.AccessTokenProvider = () => Task.FromResult(token); }).Build();

		Debug.WriteLine("Connecting...");
		await _connection.StartAsync();
		Debug.WriteLine(_connection.State.ToString());

		_startTime = DateTime.UtcNow;
		_dataUpdate.Enabled = true;

		ToastManager?.Show(
			new Toast("Started charging."),
			showIcon: true,
			showClose: false,
			type: NotificationType.Success,
			classes: ["Light"]);
	}

	private async Task UpdateData()
	{
		if (Finished)
			return;

		var car = Cars[PickedIndex];
		var chargingRate = CurrentStation.MaxChargeRate > car.ChargingRate ? car.ChargingRate : CurrentStation.MaxChargeRate;

		var time = DateTime.UtcNow - _startTime;
		Time = time.ToString("hh\\:mm\\:ss");
		TotalCharge = Math.Round(chargingRate * time.TotalSeconds / 1000, 2); //per kwh
		TotalCost = Math.Round(TotalCharge * CurrentStation.Cost, 2);

		if (DeclaredAmount && TotalCharge >= Amount)
		{
			Finished = true;
			_dataUpdate.Enabled = false;

			if (_connection == null)
				return;

			await _connection.StopAsync();
			Debug.WriteLine(_connection.State.ToString());
		}
	}

	public async Task Stop()
	{
		if (_connection == null)
			return;

		bool isDiscount = await _connection.InvokeAsync<bool>("RequestClose");
		await _connection.StopAsync();

		Debug.WriteLine(isDiscount);

		Finished = true;
		_dataUpdate.Enabled = false;

		Debug.WriteLine(_connection.State.ToString());

		ToastManager?.Show(
			new Toast("Stopped charging."),
			showIcon: true,
			showClose: false,
			type: NotificationType.Success,
			classes: ["Light"]);

		HostScreen.Router.NavigateAndReset.Execute(new TransactionViewModel(isDiscount, HostScreen));
	}

	//navigation
	public void NavigateHome() => HostScreen.Router.NavigateAndReset.Execute(new MapViewModel(HostScreen));
}