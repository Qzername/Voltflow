using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace VoltflowAPI.Hubs;

[AllowAnonymous]
public class PiChargingHub : Hub
{
    private bool _enabled;

    public override async Task OnConnectedAsync()
    {
        Debug.WriteLine("Connection opened");

        while (true)
        {
            await Task.Delay(1000);
            await Clients.Caller.SendAsync(_enabled ? "TurnPortOff" : "TurnPortOn", 0);
            _enabled = !_enabled;
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        Debug.WriteLine("Connection closed");
    }
}
