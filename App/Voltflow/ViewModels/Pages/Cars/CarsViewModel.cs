using Avalonia.Collections;
using ReactiveUI;
using System.Diagnostics;
using System.Net.Http;
using Voltflow.Models;

namespace Voltflow.ViewModels.Pages.Cars;

public class CarsViewModel : ViewModelBase
{
    AvaloniaList<Car> cars { get; set; } = new();

    public CarsViewModel(IScreen screen) : base(screen)
    {
        GetCars();
    }

    async void GetCars()
    {
        cars.Clear();

        var httpClient = GetService<HttpClient>();
        var response = await httpClient.GetAsync("/api/Cars");
        var jsonString = await response.Content.ReadAsStringAsync();
        Debug.WriteLine(response.StatusCode);

        var carsObjs = JsonConverter.Deserialize<Car[]>(jsonString);
        cars.AddRange(carsObjs);
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
