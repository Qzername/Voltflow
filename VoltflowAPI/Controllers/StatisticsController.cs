using Microsoft.AspNetCore.Mvc;
using VoltflowAPI.Services;

namespace VoltflowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    [HttpGet("ChargingStations/weekUsage")]
    public IActionResult GetWeekUsage([FromQuery] int stationId)
    {
        if (!StatisticsUpdateService.ChargingStationWeekUsage.ContainsKey(stationId))
            return BadRequest();

        return Ok(StatisticsUpdateService.ChargingStationWeekUsage[stationId]);
    }

    [HttpGet("ChargingStations/peekHours")]
    public IActionResult GetPeekHours([FromQuery] int stationId)
    {
        if (!StatisticsUpdateService.ChargingStationPeekHours.ContainsKey(stationId))
            return BadRequest();

        return Ok(StatisticsUpdateService.ChargingStationPeekHours[stationId]);
    }
}
