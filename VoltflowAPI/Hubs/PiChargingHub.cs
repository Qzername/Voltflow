using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Claims;
using VoltflowAPI.Contexts;
using VoltflowAPI.Models;
using VoltflowAPI.Models.Application;

namespace VoltflowAPI.Hubs;

[AllowAnonymous]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class PiChargingHub : Hub
{
    static readonly ConcurrentDictionary<string, ChargingInfo> _connections = new();

    readonly UserManager<Account> _userManager;
    readonly ApplicationContext _applicationContext;

    public PiChargingHub(UserManager<Account> userManager, ApplicationContext applicationContext)
    {
        _userManager = userManager;
        _applicationContext = applicationContext;
    }

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
