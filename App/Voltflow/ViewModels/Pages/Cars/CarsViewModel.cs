using Avalonia.Collections;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Voltflow.Models;

namespace Voltflow.ViewModels.Pages.Cars;

public class CarsViewModel : ViewModelBase
{
	[Reactive] public AvaloniaList<Car> Cars { get; set; } = [];

	public CarsViewModel(IScreen screen) : base(screen)
	{
		GetCars();
	}

	private async void GetCars()
	{
		Cars.Clear();

		var httpClient = GetService<HttpClient>();
		var response = await httpClient.GetAsync("/api/Cars");
		Debug.WriteLine(response.StatusCode);

		if (response.StatusCode != HttpStatusCode.OK)
			return;

		var jsonString = await response.Content.ReadAsStringAsync();

		var carsObjs = JsonConverter.Deserialize<Car[]>(jsonString);
		Cars.AddRange(carsObjs);
	}

	public void NavigateToCarDetails(object carObj)
	{
		HostScreen.Router.Navigate.Execute(new CarInfoViewModel((Car)carObj, HostScreen));
	}

	public void NavigateToCreateCar()
	{
		HostScreen.Router.Navigate.Execute(new AddCarViewModel(HostScreen));
	}
}
