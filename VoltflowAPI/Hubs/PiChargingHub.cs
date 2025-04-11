using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Diagnostics;
using VoltflowAPI.Contexts;
using VoltflowAPI.Models.Application;
using VoltflowAPI.Services;

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

    public async Task SetOutOfService()
    {
        var chargingStation = _applicationContext.ChargingStations.Find(_connections[Context.ConnectionId].Id);
        var ports = _applicationContext.ChargingPorts.Where(p => p.StationId == chargingStation.Id).OrderBy(x => x.Id).ToList();

        var portOne = ports[0];
        portOne.Status = (int)ChargingPortStatus.OutOfService;
        _applicationContext.Update(portOne);

        var portTwo = ports[1];
        portTwo.Status = (int)ChargingPortStatus.OutOfService;
        _applicationContext.Update(portTwo);

        await _applicationContext.SaveChangesAsync();
        await _applicationContext.DisposeAsync();
    }

    public void SetWattage(int index, float wattage)
    {
        var port = _connections[Context.ConnectionId].Ports.OrderBy(x => x.Id).ToList()[index];

        ChargeManager.Wattages[port.Id] = (float)Math.Round(wattage) < 1 ? 0 : (float)Math.Round(wattage);
        Console.WriteLine($"SetWattage: {port.Id} (slot {index}) - {ChargeManager.Wattages[port.Id]}W");
    }

    public async void GetPort(int index)
    {
        var chargingStation = _applicationContext.ChargingStations.Find(_connections[Context.ConnectionId].Id);

        if (chargingStation is null)
        {
            await Clients.Caller.SendAsync("Error", "Station not found");
            Context.Abort();
        }

        var ports = _applicationContext.ChargingPorts.Where(p => p.StationId == chargingStation.Id).OrderBy(x => x.Id).ToList();
        chargingStation.Ports = ports;
        _connections[Context.ConnectionId] = chargingStation;

        await Clients.Caller.SendAsync("Port", new
        {
            Index = index,
            Status = ports[index].Status,
            ServiceMode = ports[index].ServiceMode,
            Time = ChargeManager.StartDates.ContainsKey(ports[index].Id) ? (ChargeManager.StartDates[ports[index].Id] - DateTime.UtcNow).ToString("hh\\:mm\\:ss") : null
        });
    }

    public async void GetMessage()
    {
        var chargingStation = _applicationContext.ChargingStations.Find(_connections[Context.ConnectionId].Id);

        if (chargingStation is null)
        {
            await Clients.Caller.SendAsync("Error", "Station not found");
            Context.Abort();
        }

        _connections[Context.ConnectionId] = chargingStation;

        await Clients.Caller.SendAsync("Message", chargingStation.Message);
    }

    public async void GetStation()
    {
        var chargingStation = _applicationContext.ChargingStations.Find(_connections[Context.ConnectionId].Id);

        if (chargingStation is null)
        {
            await Clients.Caller.SendAsync("Error", "Station not found");
            Context.Abort();
        }

        _connections[Context.ConnectionId] = chargingStation;

        await Clients.Caller.SendAsync("Station", new
        {
            Cost = chargingStation.Cost,
            MaxChargeRate = chargingStation.MaxChargeRate
        });
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

        var connInfo = _connections[Context.ConnectionId];
        foreach (var port in connInfo.Ports)
            ChargeManager.Wattages.TryRemove(port.Id, out _);

        _connections.TryRemove(Context.ConnectionId, out _);
    }
}
