using Microsoft.AspNetCore.SignalR.Client;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Voltflow.ViewModels.Pages.Charging
{
    public class ChargingViewModel : ViewModelBase
    {
        public ChargingViewModel(IScreen screen) : base(screen)
        {
        }

        public async Task Start()
        {
            try
            {
                var connection = new HubConnectionBuilder().WithUrl("ws://localhost:5000/charginghub").Build();

                await connection.StartAsync();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
