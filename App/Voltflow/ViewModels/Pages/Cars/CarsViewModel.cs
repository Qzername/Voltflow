using Avalonia.Collections;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Net;
using System.Net.Http;
using Voltflow.Models;

namespace Voltflow.ViewModels.Pages.Cars;

public class CarsViewModel : ViewModelBase
{
    [Reactive] public AvaloniaList<Car> Cars { get; set; } = [];

    private readonly HttpClient _httpClient;

    public CarsViewModel(IScreen screen) : base(screen)
    {
        _httpClient = GetService<HttpClient>();
        GetCars();
    }

    private async void GetCars()
    {
        Cars.Clear();

        var request = await _httpClient.GetAsync("/api/Cars");

        if (request.StatusCode != HttpStatusCode.OK)
            return;

        var jsonString = await request.Content.ReadAsStringAsync();

        var cars = JsonConverter.Deserialize<Car[]>(jsonString);
        if (cars != null)
            Cars.AddRange(cars);

        if (Cars.Count == 0)
            NavigateToCreateCar();
    }

    public void NavigateToCarDetails(object carObj) => HostScreen.Router.Navigate.Execute(new CarInfoViewModel((Car)carObj, HostScreen));
    public void NavigateToCreateCar() => HostScreen.Router.Navigate.Execute(new AddCarViewModel(HostScreen, Cars.Count == 0));
}
