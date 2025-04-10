using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Diagnostics;
using VoltflowAPI.Contexts;
using VoltflowAPI.Models.Application;

namespace VoltflowAPI.Hubs;

[AllowAnonymous]
public class PiChargingHub : Hub
{
    static readonly ConcurrentDictionary<string, ChargingStation> _connections = new();

    readonly UserManager<Account> _userManager;
    readonly ApplicationContext _applicationContext;

    public PiChargingHub(UserManager<Account> userManager, ApplicationContext applicationContext)
    {
        _userManager = userManager;
        _applicationContext = applicationContext;
    }

    public async Task<ChargingStation> GetInfo()
    {
        var chargingStation = _applicationContext.ChargingStations.Find(_connections[Context.ConnectionId].Id);

        if (chargingStation is null)
        {
            await Clients.Caller.SendAsync("Error", "Station not found");
            Context.Abort();
        }

        _connections[Context.ConnectionId] = chargingStation;
        return chargingStation;
    }

    public override async Task OnConnectedAsync()
    {
        Debug.WriteLine("Connection opened");

        var httpContext = Context.GetHttpContext();

        if (httpContext is null || !httpContext.Request.Query.ContainsKey("password") ||
            !httpContext.Request.Query.ContainsKey("stationId"))
        {
            await Clients.Caller.SendAsync("Error", "Invalid request");
            Context.Abort();
        }

        var password = httpContext!.Request.Query["password"].ToString();
        var stationId = httpContext!.Request.Query["stationId"].ToString();
        var chargingStation = _applicationContext.ChargingStations.Find(Convert.ToInt32(stationId));

        if (chargingStation is null)
        {
            await Clients.Caller.SendAsync("Error", "Station not found");
            Context.Abort();
        }

        if (chargingStation.Password != password)
        {
            await Clients.Caller.SendAsync("Error", "Invalid password");
            Context.Abort();
        }

        _connections[Context.ConnectionId] = chargingStation;
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        Debug.WriteLine("Connection closed");

        _connections.TryRemove(Context.ConnectionId, out _);
    }
}
