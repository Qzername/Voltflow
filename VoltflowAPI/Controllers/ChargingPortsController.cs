﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoltflowAPI.Contexts;
using VoltflowAPI.Models.Application;

namespace VoltflowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ChargingPortsController : ControllerBase
{
    readonly ApplicationContext _applicationContext;

    public ChargingPortsController(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetStations([FromQuery] int stationId)
    {
        if(!_applicationContext.ChargingPorts.Any(x=>x.Equals(stationId)))
            return BadRequest(new { InvalidStation = true });

        var chargingPorts = _applicationContext.ChargingPorts.Where(x=>x.StationId == stationId);
        return Ok(chargingPorts);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePort([FromBody] CreatePortModel model)
    {
        if (!_applicationContext.ChargingPorts.Any(x => x.Equals(model.StationId)))
            return BadRequest(new { InvalidStation = true });

        var chargingPort = new ChargingPort
        {
            StationId = model.StationId,
            Name = model.Name,
            Status = (int)ChargingPortStatus.Available,
            ServiceMode = false,
        };

        await _applicationContext.ChargingPorts.AddAsync(chargingPort);
        await _applicationContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPatch]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PatchPort([FromBody] PatchPortModel model)
    {
        var chargingPort = await _applicationContext.ChargingPorts.FindAsync(model.Id);
        
        if (chargingPort is null)
            return BadRequest(new { InvalidPort = true });

        if (model.Name is not null)
            chargingPort.Name = model.Name;

        if (model.Status is not null)
            chargingPort.Status = model.Status.Value;

        if (model.ServiceMode is not null)
            chargingPort.ServiceMode = model.ServiceMode.Value;
      
        _applicationContext.Update(chargingPort);
        await _applicationContext.SaveChangesAsync();
        return Ok();
    }

    public struct CreatePortModel
    {
        public int StationId { get; set; }
        public string Name { get; set; }
    }

    public struct PatchPortModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? Status { get; set; }
        public bool? ServiceMode { get; set; }
    }
}