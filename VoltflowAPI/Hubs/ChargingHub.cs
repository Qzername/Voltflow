using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;
using VoltflowAPI.Contexts;
using VoltflowAPI.Models;
using VoltflowAPI.Models.Application;
using VoltflowAPI.Services;

namespace VoltflowAPI.Hubs;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ChargingHub : Hub
{
    static readonly ConcurrentDictionary<string, ChargingInfo> _connections = new();

    readonly UserManager<Account> _userManager;
    readonly ApplicationContext _applicationContext;

    public ChargingHub(UserManager<Account> userManager, ApplicationContext applicationContext)
    {
        _userManager = userManager;
        _applicationContext = applicationContext;
    }

    public async Task<bool> RequestClose()
    {
        var rng = new Random();
        bool isDiscount = rng.Next(0, 5) == 0;

        var conn = _connections[Context.ConnectionId];
        conn.IsDiscount = isDiscount;
        _connections[Context.ConnectionId] = conn;

        return isDiscount;
    }

    public async Task<bool> IsDisconnected()
    {
        var conn = _connections[Context.ConnectionId];
        return conn.Disconnected;
    }

    public async Task<double[]> GetChargingInfo()
    {
        var connInfo = _connections[Context.ConnectionId];

        if (connInfo.Disconnected)
            return [0, 0];

        var timePassed = DateTime.UtcNow - connInfo.LastUpdated;
        connInfo.LastUpdated = DateTime.UtcNow;

        var station = _applicationContext.ChargingStations.Find(connInfo.StationId);

        float chargingRate = 0;
        if (WattageManager.Wattages.ContainsKey(connInfo.PortId))
            chargingRate = WattageManager.Wattages[connInfo.PortId];
        connInfo.EnergyConsumed += Math.Round(chargingRate * timePassed.TotalSeconds / 1000, 3);
        connInfo.TotalCost += Math.Round(connInfo.EnergyConsumed * station.Cost, 2);

        Console.WriteLine($"ChargingRate: {chargingRate}");
        Console.WriteLine($"EnergyConsumed: {connInfo.EnergyConsumed}");
        Console.WriteLine($"TotalCost: {connInfo.TotalCost}");

        if (chargingRate < 1 && connInfo.EnergyConsumed > 0)
            connInfo.Disconnected = true;

        _connections[Context.ConnectionId] = connInfo;
        return [connInfo.EnergyConsumed, connInfo.TotalCost];
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine("Connection started");

        var httpContext = Context.GetHttpContext();

        if (httpContext is null ||
            !httpContext.Request.Query.ContainsKey("portId") ||
            !httpContext.Request.Query.ContainsKey("carId"))
        {
            await Clients.Caller.SendAsync("Error", "Invalid request");
            Context.Abort();
        }

        var portId = httpContext!.Request.Query["portId"].ToString();
        var carIdString = httpContext.Request.Query["carId"].ToString();
        int carId = 0;

        if (!int.TryParse(carIdString, out carId))
        {
            await Clients.Caller.SendAsync("Error", "Invalid request");
            Context.Abort();
        }

        var port = _applicationContext.ChargingPorts.Find(Convert.ToInt32(portId));

        if (port is null)
        {
            await Clients.Caller.SendAsync("Error", "Station not found");
            Context.Abort();
        }

        if (port.Status != (int)ChargingPortStatus.Available)
        {
            await Clients.Caller.SendAsync("Error", "Station is not available");
            Context.Abort();
        }

        var station = _applicationContext.ChargingStations.Find(port.StationId);
        var openingHours = _applicationContext.ChargingStationOpeningHours.FirstOrDefault(x => x.StationId == station.Id);

        var hours = openingHours.GetToday();

        if (DateTime.UtcNow.TimeOfDay < hours[0] ||
           DateTime.UtcNow.TimeOfDay > hours[1])
        {
            await Clients.Caller.SendAsync("Error", "Station is not available");
            Context.Abort();
        }

        port.Status = (int)ChargingPortStatus.Occupied;
        _applicationContext.Update(port);
        await _applicationContext.SaveChangesAsync();

        var utcNow = DateTime.UtcNow;

        await Clients.Caller.SendAsync("Ok", utcNow);
        _connections[Context.ConnectionId] = new ChargingInfo()
        {
            StartDate = utcNow,
            LastUpdated = utcNow,
            PortId = port.Id,
            StationId = station.Id,
            CarId = carId
        };

        Console.WriteLine("Charging started");
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var id = Context.User.Claims.ToList().Single(x => x.Type == ClaimTypes.NameIdentifier);

        //get user
        var userName = Context.User?.Identity.Name;
        var user = await _userManager.FindByIdAsync(id.Value);

        //register time
        var connInfo = _connections[Context.ConnectionId];
        var endTime = DateTime.UtcNow;

        //set port status to available
        var port = _applicationContext.ChargingPorts.Find(connInfo.PortId);
        port.Status = (int)ChargingPortStatus.Available;
        _applicationContext.Update(port);
        await _applicationContext.SaveChangesAsync();

        //remove connection
        _connections.TryRemove(Context.ConnectionId, out _);

        //get car and station
        var station = _applicationContext.ChargingStations.Find(connInfo.StationId);

        //register transaction
        Transaction transaction = new()
        {
            AccountId = user.Id,
            CarId = connInfo.CarId,
            ChargingStationId = connInfo.StationId,
            StartDate = connInfo.StartDate,
            EndDate = endTime,
            EnergyConsumed = Math.Round(connInfo.EnergyConsumed, 3),
            Cost = Math.Round(connInfo.IsDiscount ? connInfo.TotalCost * 0.9 : connInfo.TotalCost, 2)
        };

        _applicationContext.Transactions.Add(transaction);
        await _applicationContext.SaveChangesAsync();

    }
}
