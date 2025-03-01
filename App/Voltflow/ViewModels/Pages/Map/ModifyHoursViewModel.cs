using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Diagnostics;
using System.Net.Http;
using Voltflow.Models;

namespace Voltflow.ViewModels.Pages.Map;

public class ModifyHoursViewModel : ViewModelBase
{
    HttpClient _httpClient;

    ChargingStation _chargingStation;
    [Reactive] public ChargingStationOpeningHours OpeningHours { get; set; }

    public ModifyHoursViewModel(ChargingStation station, ChargingStationOpeningHours hours, IScreen screen) : base(screen)
    {
        _httpClient = GetService<HttpClient>();

        _chargingStation = station;
        OpeningHours = hours;
    }

    public async void Update()
    {
        var body = JsonConverter.ToStringContent(OpeningHours);

        var request = await _httpClient.PatchAsync("/api/ChargingStations/OpeningHours", body);
        Debug.WriteLine(request.StatusCode);
    }
}
