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

        //meet data criteria
        if (accountModel.Name?.Length > 100 || 
            accountModel.Surname?.Length > 100 || 
            accountModel.PhoneNumber?.Length != 9)
            return BadRequest(new { InvalidData = true });

        if (!string.IsNullOrEmpty(accountModel.Name))
            user.Name = accountModel.Name;

        if (!string.IsNullOrEmpty(accountModel.Surname))
            user.Surname = accountModel.Surname;

        if (!string.IsNullOrEmpty(accountModel.PhoneNumber))
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
