using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoltflowAPI.Contexts;
using VoltflowAPI.Models.Application;

namespace VoltflowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ChargingStationsController : ControllerBase
{
    readonly ApplicationContext _applicationContext;

    public ChargingStationsController(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetStations()
    {
        var chargingStations = _applicationContext.ChargingStations
            .Include(x=>x.Ports)
            .Include(x=>x.OpeningHours)
            .Select(x=>
        new {
            x.Id,
            x.Latitude,
            x.Longitude,
            x.Cost,
            x.MaxChargeRate,
            x.Password,
            x.Message,
            Ports = x.Ports.Where(y => y.StationId == x.Id).OrderBy(x => x.Id).ToArray(),
            OpeningHours = x.OpeningHours.Single(y => y.StationId == x.Id),
        }).ToArray();
        return Ok(chargingStations);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateStation([FromBody] CreateStationModel model)
    {
        //meet data criteria
        if (model.Cost < 1 || model.MaxChargeRate < 1)
            return BadRequest(new { InvalidData = true });

        ChargingStation chargingStation = new ChargingStation
        {
            Longitude = model.Longitude,
            Latitude = model.Latitude,
            Cost = model.Cost,
            MaxChargeRate = model.MaxChargeRate,
            Password = model.Password,
            Message = model.Message
        };

        var entry = await _applicationContext.ChargingStations.AddAsync(chargingStation);
        await _applicationContext.SaveChangesAsync();

        //add default times
        await _applicationContext.ChargingStationOpeningHours.AddAsync(new ChargingStationOpeningHours()
        {
            StationId = chargingStation.Id,
            Monday = [new TimeSpan(0, 0, 0), new TimeSpan(23, 59, 59)],
            Tuesday = [new TimeSpan(0, 0, 0), new TimeSpan(23, 59, 59)],
            Wednesday = [new TimeSpan(0, 0, 0), new TimeSpan(23, 59, 59)],
            Thursday = [new TimeSpan(0, 0, 0), new TimeSpan(23, 59, 59)],
            Friday = [new TimeSpan(0, 0, 0), new TimeSpan(23, 59, 59)],
            Saturday = [new TimeSpan(0, 0, 0), new TimeSpan(23, 59, 59)],
            Sunday = [new TimeSpan(0, 0, 0), new TimeSpan(23, 59, 59)],
        });
        await _applicationContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPatch]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PatchStation(PatchStationModel model)
    {
        //meet data criteria
        if ((model.Cost is not null && model.Cost < 1) ||
            (model.MaxChargeRate is not null && model.MaxChargeRate < 1))
            return BadRequest(new { InvalidData = true });

        var station = _applicationContext.ChargingStations.Single(x => x.Id == model.Id);

        if (model.Latitude is not null)
            station.Latitude = model.Latitude.Value;

        if (model.Longitude is not null)
            station.Longitude = model.Longitude.Value;

        if (model.Cost is not null)
            station.Cost = model.Cost.Value;

        if (model.MaxChargeRate is not null)
            station.MaxChargeRate = model.MaxChargeRate.Value;

        if (model.Password is not null)
            station.Password = model.Password;

        station.Message = model.Message;

        _applicationContext.Update(station);
        await _applicationContext.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteStation(int id)
    {
        var station = _applicationContext.ChargingStations.Single(x => x.Id == id);

        _applicationContext.Remove(station);
        await _applicationContext.SaveChangesAsync();

        return Ok();
    }

    public struct CreateStationModel
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public int Cost { get; set; }
        public int MaxChargeRate { get; set; }
        public string Password { get; set; }
        public string? Message { get; set; }
    }

    public struct PatchStationModel
    {
        public int Id { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public int? Cost { get; set; }
        public int? MaxChargeRate { get; set; }
        public string? Password { get; set; }
        public string? Message { get; set; }
        public ChargingPortStatus? Status { get; set; }
        public bool? ServiceMode { get; set; }
    }
}
