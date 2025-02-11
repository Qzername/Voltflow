using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace VoltflowAPI.Controllers;

#if DEBUG

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    readonly UserManager<Account> _userManager;

    public TestController(UserManager<Account> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost("GiveAdmin")]
    public async Task<IActionResult> GiveAdmin([FromBody] string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        await _userManager.AddToRoleAsync(user, "Admin");
        return Ok();
    }
}

#endif