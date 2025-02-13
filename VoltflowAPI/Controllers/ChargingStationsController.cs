using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoltflowAPI.Contexts;
using VoltflowAPI.Models;

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
        var chargingStations = await _applicationContext.ChargingStations.ToArrayAsync();
        return Ok(chargingStations);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateStation([FromBody] CreateStationModel model)
    {
        ChargingStation chargingStation = new ChargingStation
        {
            Longitude = model.Longitude,
            Latitude = model.Latitude,
            Cost = model.Cost,
            MaxChargeRate = model.MaxChargeRate,

            Status = (int)ChargingStationStatus.Available,
            ServiceMode = false
        };

        await _applicationContext.ChargingStations.AddAsync(chargingStation);
        await _applicationContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPatch]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PatchStation(PatchStationModel patchStationModel)
    {
        var station = _applicationContext.ChargingStations.Single(x=>x.Id == patchStationModel.Id);

        if(patchStationModel.Latitude is not null)
            station.Latitude = patchStationModel.Latitude.Value;

        if (patchStationModel.Longitude is not null)
            station.Longitude = patchStationModel.Longitude.Value;

        if (patchStationModel.Cost is not null)
            station.Cost = patchStationModel.Cost.Value;

        if (patchStationModel.MaxChargeRate is not null)
            station.MaxChargeRate = patchStationModel.MaxChargeRate.Value;

        if (patchStationModel.Status is not null)
            station.Status = (int)patchStationModel.Status.Value;

        if (patchStationModel.ServiceMode is not null)
            station.ServiceMode = patchStationModel.ServiceMode.Value;

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
    }

    public struct PatchStationModel
    {
        public int Id { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public int? Cost { get; set; }
        public int? MaxChargeRate { get; set; }
        public ChargingStationStatus? Status { get; set; }
        public bool? ServiceMode { get; set; }
    }
}
