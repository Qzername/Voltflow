﻿using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using Avalonia.SimplePreferences;
using Avalonia.Threading;
using Microsoft.AspNetCore.SignalR.Client;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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

	// Started will be always true after first start.
	// It's used for displaying charging info.
	// Finished determines whether car is done charging.
	// Working determines whether functions Start() or Stop() are running.
	[Reactive] public bool Started { get; set; }
	[Reactive] public bool Finished { get; set; } = true;
	[Reactive] public bool Working { get; set; }

	private HubConnection? _connection;
	private readonly DispatcherTimer _dataUpdate = new();
	private DateTime _startTime;
	private readonly HttpClient _httpClient;

	public ChargingViewModel(ChargingStation chargingStation, IScreen screen) : base(screen)
	{
		_httpClient = GetService<HttpClient>();
		CurrentStation = chargingStation;

		_dataUpdate.Interval = TimeSpan.FromMilliseconds(100);
		_dataUpdate.Tick += async (sender, e) => await UpdateData();

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

		if (Working)
			return;

		Working = true;

		Time = "00:00:00";
		TotalCharge = 0;
		TotalCost = 0;

		var token = await Preferences.GetAsync<string?>("token", null);
		var carId = Cars[PickedIndex].Id;

		_connection = new HubConnectionBuilder()
			.WithUrl($"https://voltflow-api.heapy.xyz/charginghub?carId={carId}&stationId={CurrentStation.Id}",
				(options) => { options.AccessTokenProvider = () => Task.FromResult(token); }).Build();

		await _connection.StartAsync();

		_startTime = DateTime.UtcNow;
		_dataUpdate.IsEnabled = true;
		Started = true;
		Finished = false;

		ToastManager?.Show(
			new Toast("Started charging."),
			showIcon: true,
			showClose: false,
			type: NotificationType.Success,
			classes: ["Light"]);

		Working = false;
	}

	private async Task UpdateData()
	{
		if (!_dataUpdate.IsEnabled)
			return;

		var car = Cars[PickedIndex];
		var chargingRate = CurrentStation.MaxChargeRate > car.ChargingRate ? car.ChargingRate : CurrentStation.MaxChargeRate;

		var time = DateTime.UtcNow - _startTime;
		Time = time.ToString("hh\\:mm\\:ss");
		TotalCharge = Math.Round(chargingRate * time.TotalSeconds / 1000, 2); //per kwh
		TotalCost = Math.Round(TotalCharge * CurrentStation.Cost, 2);

		if (DeclaredAmount && TotalCharge >= Amount)
		{
			if (!_dataUpdate.IsEnabled || _connection == null)
				return;

			_dataUpdate.IsEnabled = false;

			bool isDiscount = await _connection.InvokeAsync<bool>("RequestClose");
			await _connection.StopAsync();

			HostScreen.Router.NavigateAndReset.Execute(new TransactionViewModel(isDiscount, HostScreen));

			Finished = true;
		}
	}

	public async Task Stop()
	{
		if (!_dataUpdate.IsEnabled || _connection == null || Working)
			return;

		Working = true;
		_dataUpdate.IsEnabled = false;

		bool isDiscount = await _connection.InvokeAsync<bool>("RequestClose");
		await _connection.StopAsync();

		HostScreen.Router.NavigateAndReset.Execute(new TransactionViewModel(isDiscount, HostScreen));

		Finished = true;
		Working = false;
	}

	//navigation
	public void NavigateHome() => HostScreen.Router.NavigateAndReset.Execute(new MapViewModel(HostScreen));
}