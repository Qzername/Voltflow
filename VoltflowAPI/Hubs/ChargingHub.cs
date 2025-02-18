using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;
using VoltflowAPI.Contexts;
using VoltflowAPI.Models;
using VoltflowAPI.Models.Identity;

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

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var stationId = httpContext.Request.Query["stationId"].ToString();

        var station = _applicationContext.ChargingStations.Find(Convert.ToInt32(stationId));

        Console.WriteLine("Connection started");

        if (station is null)
        {
            await Clients.Caller.SendAsync("Error", "Station not found");
            Context.Abort();
        }

        if (station.Status != (int)ChargingStationStatus.Available)
        {
            await Clients.Caller.SendAsync("Error", "Station is not available");
            Context.Abort();
        }

        station.Status = (int)ChargingStationStatus.Occupied;
        _applicationContext.Update(station);
        await _applicationContext.SaveChangesAsync();

        var utcNow = DateTime.UtcNow;

        await Clients.Caller.SendAsync("Ok", utcNow);
        _connections[Context.ConnectionId] = new ChargingInfo()
        {
            StartDate = utcNow,
            StationId = station.Id
        };

        Console.WriteLine("Charging started");
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var id = Context.User.Claims.ToList().Single(x=>x.Type == ClaimTypes.NameIdentifier);

        //get user
        var userName = Context.User?.Identity.Name;
        var user = await _userManager.FindByIdAsync(id.Value);

        //register time
        var connInfo = _connections[Context.ConnectionId];
        var endTime = DateTime.UtcNow;
        var timePassed = endTime - connInfo.StartDate;

        //set station status to available
        var station = _applicationContext.ChargingStations.Find(connInfo.StationId);
        station.Status = (int)ChargingStationStatus.Available;
        _applicationContext.Update(station);
        await _applicationContext.SaveChangesAsync();

        //remove connection
        _connections.TryRemove(Context.ConnectionId, out _);

        //register transaction
        Transaction transaction = new()
        {
            AccountId = user.Id,
            CarId = 1,
            ChargingStationId = connInfo.StationId,
            StartDate = connInfo.StartDate,
            EndDate = endTime,
            EnergyConsumed = 100,
            Cost = 100
        };

        _applicationContext.Transactions.Add(transaction);
        await _applicationContext.SaveChangesAsync();

        Console.WriteLine(timePassed);
    }
}
