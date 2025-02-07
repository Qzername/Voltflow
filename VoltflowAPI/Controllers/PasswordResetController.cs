using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VoltflowAPI.Models.Endpoints;
using VoltflowAPI.Services;

namespace VoltflowAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class PasswordResetController : ControllerBase
{
    readonly UserManager<Account> _userManager;
    readonly IEmailSender _emailSender;

    public PasswordResetController(UserManager<Account> userManager, IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    [HttpGet("send")]
    public async Task<IActionResult> SendToken()
    {
        var claims = User.Claims.ToList();
        var email = claims.Single(x => x.Type == ClaimTypes.Email).Value;

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
            return Unauthorized();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var message = $"Your password reset token is: {token}";
        await _emailSender.SendEmailAsync(user.Email, "Password reset", message);

        return Ok();
    }

    [HttpPost("reset")]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordResetModel password)
    {
        var claims = User.Claims.ToList();
        var email = claims.Single(x => x.Type == ClaimTypes.Email).Value;

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
            return Unauthorized();

        var result = await _userManager.ResetPasswordAsync(user, password.TokenModel.Token, password.Password);

        if(!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }

    public struct PasswordResetModel
    {
        public string Password { get; set; }
        public TokenModel TokenModel { get; set; }
    }
}
