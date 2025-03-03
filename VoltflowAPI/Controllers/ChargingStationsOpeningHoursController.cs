using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoltflowAPI.Contexts;
using VoltflowAPI.Models.Application;

namespace VoltflowAPI.Controllers;

[ApiController]
[Route("api/ChargingStations/OpeningHours")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ChargingStationsOpeningHoursController : Controller
{
    readonly ApplicationContext _applicationContext;

    public ChargingStationsOpeningHoursController(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetOpeningHours([FromQuery] int stationId)
    {
        var openingHours = _applicationContext.ChargingStationOpeningHours.Single(x => x.StationId == stationId);
        return Ok(openingHours);
    }

    [HttpPatch]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetOpeningHours([FromBody] SetOpeningHoursModel model)
    {
        var openingHours = _applicationContext.ChargingStationOpeningHours.FirstOrDefault(x => x.StationId == model.StationId);
        if (openingHours == null)
        {
            openingHours = new ChargingStationOpeningHours()
            {
                StationId = model.StationId,
                Monday = model.Monday,
                Tuesday = model.Tuesday,
                Wednesday = model.Wednesday,
                Thursday = model.Thursday,
                Friday = model.Friday,
                Saturday = model.Saturday,
                Sunday = model.Sunday,
            };
            await _applicationContext.ChargingStationOpeningHours.AddAsync(openingHours);
        }
        else
        {
            openingHours.Monday = model.Monday;
            openingHours.Tuesday = model.Tuesday;
            openingHours.Wednesday = model.Wednesday;
            openingHours.Thursday = model.Thursday;
            openingHours.Friday = model.Friday;
            openingHours.Saturday = model.Saturday;
            openingHours.Sunday = model.Sunday;
        }
        await _applicationContext.SaveChangesAsync();
        return Ok();
    }

    public struct SetOpeningHoursModel
    {
        public int StationId { get; set; }
        public TimeSpan[] Monday { get; set; }
        public TimeSpan[] Tuesday { get; set; }
        public TimeSpan[] Wednesday { get; set; }
        public TimeSpan[] Thursday { get; set; }
        public TimeSpan[] Friday { get; set; }
        public TimeSpan[] Saturday { get; set; }
        public TimeSpan[] Sunday { get; set; }
    }
}
