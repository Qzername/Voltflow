﻿using Microsoft.AspNetCore.SignalR.Client;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Voltflow.ViewModels.Pages.Map;

namespace Voltflow.ViewModels.Pages.Charging
{
    public class ChargingViewModel(IScreen screen) : ViewModelBase(screen)
    {
	    public void NavigateHome() => HostScreen.Router.NavigateAndReset.Execute(new MapViewModel(screen));

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
