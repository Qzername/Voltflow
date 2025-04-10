using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace VoltflowAPI.Hubs;

[AllowAnonymous]
public class PiChargingHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        Debug.WriteLine("Connection opened");
        await Clients.Caller.SendAsync("TurnPortOff", 0);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        Debug.WriteLine("Connection closed");
    }
}
