using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Threading.Tasks;
using Voltflow.Models;

namespace Voltflow.ViewModels.Pages.Map;

public class ModifyHoursViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, IRoutableViewModel> GoBack => HostScreen.Router.NavigateBack;

    private readonly HttpClient _httpClient;

    private ChargingStation _chargingStation;
    [Reactive] public ChargingStationOpeningHours OpeningHours { get; set; }
    [Reactive] public bool Working { get; set; }

    public ModifyHoursViewModel(ChargingStation station, ChargingStationOpeningHours hours, IScreen screen) : base(screen)
    {
        _httpClient = GetService<HttpClient>();

        _chargingStation = station;
        OpeningHours = hours;
    }

    public async Task Update()
    {
        Working = true;
        var body = JsonConverter.ToStringContent(OpeningHours);

        var request = await _httpClient.PatchAsync("/api/ChargingStations/OpeningHours", body);

        if (request.StatusCode == HttpStatusCode.OK)
            HostScreen.Router.NavigateBack.Execute();
        else
            Working = false; // If we're navigating back, there's no need to manage Working
    }
}
