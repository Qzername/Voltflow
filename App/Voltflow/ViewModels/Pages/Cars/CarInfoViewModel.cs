using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Net.Http;
using Voltflow.Models;

namespace Voltflow.ViewModels.Pages.Cars
{
	public class CarInfoViewModel : ViewModelBase
	{
		HttpClient _httpClient;

		[Reactive] string Name { get; set; }
		[Reactive] int BatteryCapacity { get; set; }
		[Reactive] int ChargingRate { get; set; }

		Car currentCar;

		public CarInfoViewModel(Car car, IScreen screen) : base(screen)
		{
			_httpClient = GetService<HttpClient>();

			Name = car.Name;
			BatteryCapacity = car.BatteryCapacity;
			ChargingRate = car.ChargingRate;

			currentCar = car;
		}

		public async void Update()
		{
			currentCar.Name = Name;
			currentCar.BatteryCapacity = BatteryCapacity;
			currentCar.ChargingRate = ChargingRate;

			await _httpClient.PatchAsync("/api/Cars", JsonConverter.ToStringContent(currentCar));
		}

		public async void Delete()
		{
			await _httpClient.DeleteAsync("/api/Cars?id=" + currentCar.Id);
			HostScreen.Router.Navigate.Execute(new CarsViewModel(HostScreen));
		}
	}
}
