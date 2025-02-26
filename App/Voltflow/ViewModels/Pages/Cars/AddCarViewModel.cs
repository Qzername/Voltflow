using Avalonia.Controls.Notifications;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Threading.Tasks;
using Ursa.Controls;
using Voltflow.Models;

namespace Voltflow.ViewModels.Pages.Cars;

public class AddCarViewModel : ViewModelBase
{
	public AddCarViewModel(IScreen screen) : base(screen)
	{
		_httpClient = GetService<HttpClient>();
	}

	private readonly HttpClient _httpClient;

	public ReactiveCommand<Unit, IRoutableViewModel> GoBack => HostScreen.Router.NavigateBack;
	public WindowToastManager? ToastManager;

	[Reactive] public string? Name { get; set; }
	[Reactive] public int BatteryCapacity { get; set; }
	[Reactive] public int ChargingRate { get; set; }

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

		if (BatteryCapacity <= 0)
		{
			ToastManager?.Show(
				new Toast("Battery capacity must be above 0!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Error,
				classes: ["Light"]);
			return;
		}

		if (ChargingRate <= 0)
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
			BatteryCapacity = BatteryCapacity,
			ChargingRate = ChargingRate
		};

		var request = await _httpClient.PostAsync("/api/Cars", JsonConverter.ToStringContent(car));
		if (request.StatusCode == HttpStatusCode.OK)
			HostScreen.Router.Navigate.Execute(new CarsViewModel(HostScreen));
	}
}
