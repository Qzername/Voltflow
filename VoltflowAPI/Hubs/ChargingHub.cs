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

    public async Task<bool> RequestClose()
    {
        var rng = new Random();
        bool isDiscount = rng.Next(0, 5) == 0;

        var conn = _connections[Context.ConnectionId];
        conn.IsDiscount = isDiscount;
        _connections[Context.ConnectionId] = conn;
        
        return isDiscount;
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine("Connection started");

        var httpContext = Context.GetHttpContext();

        if (httpContext is null ||
            !httpContext.Request.Query.ContainsKey("stationId") ||
            !httpContext.Request.Query.ContainsKey("carId"))
        {
            await Clients.Caller.SendAsync("Error", "Invalid request");
            Context.Abort();
        }

        var stationId = httpContext!.Request.Query["stationId"].ToString();
        var carIdString = httpContext.Request.Query["carId"].ToString();
        int carId = 0;

        if(!int.TryParse(carIdString, out carId))
        {
            await Clients.Caller.SendAsync("Error", "Invalid request");
            Context.Abort();
        }

        var station = _applicationContext.ChargingStations.Find(Convert.ToInt32(stationId));
        
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
            StationId = station.Id,
            CarId = carId
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

        //get car
        var car = _applicationContext.Cars.Find(connInfo.CarId);

        //calculactions
        var chargingRate = station.MaxChargeRate > car.ChargingRate ? car.ChargingRate : station.MaxChargeRate;
        var energyConsumed = chargingRate * timePassed.TotalSeconds / 1000;
        var totalCost = energyConsumed * station.Cost;

        //register transaction
        Transaction transaction = new()
        {
	        AccountId = user.Id,
	        CarId = connInfo.CarId,
	        ChargingStationId = connInfo.StationId,
	        StartDate = connInfo.StartDate,
	        EndDate = endTime,
	        EnergyConsumed = energyConsumed,
	        Cost = connInfo.IsDiscount ? totalCost * 0.9 : totalCost
        };

        _applicationContext.Transactions.Add(transaction);
        await _applicationContext.SaveChangesAsync();

        Console.WriteLine(timePassed.ToString("hh\\:mm\\:ss"));
    }
}
