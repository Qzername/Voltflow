using Microsoft.AspNetCore.Mvc;
using VoltflowAPI.Services;

namespace VoltflowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    [HttpGet("ChargingStations/rushHours")]
    public IActionResult GetRushHours([FromQuery] int stationId)
    {
        if (!StatisticsUpdateService.ChargingStationRushHours.ContainsKey(stationId))
            return BadRequest();

        return Ok(StatisticsUpdateService.ChargingStationRushHours[stationId]);
    }
}
