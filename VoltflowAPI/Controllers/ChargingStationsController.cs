using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VoltflowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ChargingStationsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetStations()
    {
        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateStation()
    {
        return Ok();
    }

    [HttpPatch]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PatchStation()
    {
        return Ok();
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteStation()
    {
        return Ok();
    }
}
