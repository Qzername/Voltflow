using Avalonia.Controls.Notifications;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Ursa.Controls;
using Voltflow.Models;

namespace Voltflow.ViewModels.Pages.Cars;

public class AddCarViewModel : ViewModelBase
{
    public AddCarViewModel(IScreen screen, bool noCars) : base(screen)
    {
        _httpClient = GetService<HttpClient>();
        NoCars = noCars;
    }

    private readonly HttpClient _httpClient;
    public readonly bool NoCars;

    public WindowToastManager? ToastManager;

    [Reactive] public string? Name { get; set; }
    [Reactive] public int? BatteryCapacity { get; set; }
    [Reactive] public int? ChargingRate { get; set; }

    public async Task AddCar()
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

        if (BatteryCapacity == null || BatteryCapacity <= 0)
        {
            ToastManager?.Show(
                new Toast("Battery capacity must be above 0!"),
                showIcon: true,
                showClose: false,
                type: NotificationType.Error,
                classes: ["Light"]);
            return;
        }

        if (ChargingRate == null || ChargingRate <= 0)
        {
            ToastManager?.Show(
                new Toast("Charging rate must be above 0!"),
                showIcon: true,
                showClose: false,
                type: NotificationType.Error,
                classes: ["Light"]);
            return;
        }

        Car car = new()
        {
            Name = Name,
            BatteryCapacity = (int)BatteryCapacity,
            ChargingRate = (int)ChargingRate
        };

        var request = await _httpClient.PostAsync("/api/Cars", JsonConverter.ToStringContent(car));
        if (request.StatusCode == HttpStatusCode.OK)
            HostScreen.Router.Navigate.Execute(new CarsViewModel(HostScreen));
    }
}
