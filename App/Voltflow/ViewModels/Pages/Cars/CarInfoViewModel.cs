using Avalonia.Controls.Notifications;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Threading.Tasks;
using Ursa.Controls;
using Voltflow.Models;

namespace Voltflow.ViewModels.Pages.Cars;

public class CarInfoViewModel : ViewModelBase
{
	public ReactiveCommand<Unit, IRoutableViewModel> GoBack => HostScreen.Router.NavigateBack;
	public WindowToastManager? ToastManager;
	private readonly HttpClient _httpClient;

	[Reactive] public string Name { get; set; }
	[Reactive] public int BatteryCapacity { get; set; }
	[Reactive] public int ChargingRate { get; set; }

	private Car _currentCar;

	public CarInfoViewModel(Car car, IScreen screen) : base(screen)
	{
		_httpClient = GetService<HttpClient>();

		Name = car.Name;
		BatteryCapacity = car.BatteryCapacity;
		ChargingRate = car.ChargingRate;

		_currentCar = car;
	}

	public async Task Update()
	{
		if (string.IsNullOrEmpty(Name))
		{
			ToastManager?.Show(
				new Toast("Name cannot be empty!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Error,
				classes: ["Light"]);
			return;
		}

		if (BatteryCapacity <= 0)
		{
			_currentCar.Name = Name;
			_currentCar.BatteryCapacity = BatteryCapacity;
			_currentCar.ChargingRate = ChargingRate;

			var response = await _httpClient.PatchAsync("/api/Cars", JsonConverter.ToStringContent(_currentCar));
			Debug.WriteLine(response.StatusCode);
			ToastManager?.Show(
				new Toast("Battery capacity cannot be below or equal 0!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Error,
				classes: ["Light"]);
			return;
		}

		if (ChargingRate <= 0)
		{
			ToastManager?.Show(
				new Toast("Charging rate cannot be below or equal 0!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Error,
				classes: ["Light"]);
			return;
		}

		_currentCar.Name = Name;
		_currentCar.BatteryCapacity = BatteryCapacity;
		_currentCar.ChargingRate = ChargingRate;

		var request = await _httpClient.PatchAsync("/api/Cars", JsonConverter.ToStringContent(_currentCar));
		if (request.StatusCode == HttpStatusCode.OK)
			HostScreen.Router.Navigate.Execute(new CarsViewModel(HostScreen));
	}

	public async Task Delete()
	{
		var response = await _httpClient.DeleteAsync("/api/Cars/" + _currentCar.Id);
		Debug.WriteLine(response.StatusCode);
		if (request.StatusCode == HttpStatusCode.OK)
			HostScreen.Router.Navigate.Execute(new CarsViewModel(HostScreen));
	}
}
