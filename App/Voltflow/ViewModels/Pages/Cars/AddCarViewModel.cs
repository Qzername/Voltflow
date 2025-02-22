using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Net.Http;
using System.Reactive;
using Voltflow.Models;

namespace Voltflow.ViewModels.Pages.Cars;

public class AddCarViewModel : ViewModelBase
{
	public ReactiveCommand<Unit, IRoutableViewModel> GoBack => HostScreen.Router.NavigateBack;

	[Reactive] public string Name { get; set; }
	[Reactive] public int BatteryCapacity { get; set; }
	[Reactive] public int ChargingRate { get; set; }

	public AddCarViewModel(IScreen screen) : base(screen)
	{
	}

	public async void AddCar()
	{
		var httpClient = GetService<HttpClient>();

		Car car = new()
		{
			Name = Name,
			BatteryCapacity = BatteryCapacity,
			ChargingRate = ChargingRate
		};

		await httpClient.PostAsync("/api/Cars", JsonConverter.ToStringContent(car));

		HostScreen.Router.Navigate.Execute(new CarsViewModel(HostScreen));
	}
}
