using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VoltflowAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthorizeTestController : ControllerBase
{
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult GetProtectedData()
    {
        return Ok(new { message = "auth ok" });
    }
}
