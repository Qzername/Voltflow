using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace VoltflowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AccountsController : ControllerBase
{
    readonly UserManager<Account> _userManager;

    public AccountsController(UserManager<Account> userManager)
    {
        _userManager = userManager;
    }

    [HttpPatch]
    public async Task<IActionResult> UpdateAccount([FromBody] PatchAccountModel accountModel)
    {
        var user = _userManager.GetUserAsync(User).Result;

        if (user is null)
            return BadRequest();

        if (accountModel.Name is not null)
            user.Name = accountModel.Name;

        if (accountModel.Surname is not null)
            user.Surname = accountModel.Surname;

        if (accountModel.PhoneNumber is not null)
            user.PhoneNumber = accountModel.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAccount()
    {
        var user = _userManager.GetUserAsync(User).Result;

        if (user is null)
            return BadRequest();

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }

    public struct PatchAccountModel
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
