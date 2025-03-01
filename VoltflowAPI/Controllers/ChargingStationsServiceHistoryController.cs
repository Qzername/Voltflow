using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoltflowAPI.Contexts;
using VoltflowAPI.Models.Application;

namespace VoltflowAPI.Controllers;

[ApiController]
[Route("api/ChargingStations/ServiceHistory")]
[Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Admin")]
public class ChargingStationsServiceHistoryController : Controller
{
    ApplicationContext _applicationContext;

    public ChargingStationsServiceHistoryController(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateTime? since)
    {
        ChargingStationsServiceHistory[] history;

        if (since is null)
            history = await _applicationContext.ChargingStationServiceHistory.ToArrayAsync();
        else
            history = await _applicationContext.ChargingStationServiceHistory.Where(x => x.EndDate > since).ToArrayAsync();

        return Ok(history);
    }
}
