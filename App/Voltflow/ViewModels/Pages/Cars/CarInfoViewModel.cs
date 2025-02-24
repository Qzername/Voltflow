using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Diagnostics;
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

			var response = await _httpClient.PatchAsync("/api/Cars", JsonConverter.ToStringContent(currentCar));
			Debug.WriteLine(response.StatusCode);
		}

		public async void Delete()
		{
			Debug.WriteLine("test");
			var response = await _httpClient.DeleteAsync("/api/Cars/" + currentCar.Id);
			Debug.WriteLine(response.StatusCode);
			Debug.WriteLine(await response.Content.ReadAsStringAsync());
			HostScreen.Router.Navigate.Execute(new CarsViewModel(HostScreen));
		}
	}
}
