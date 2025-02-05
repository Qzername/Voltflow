using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace VoltflowAPI.Controllers;

/// <summary>
/// Managers Two Factor login
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class TwoFactorController : ControllerBase
{
    readonly UserManager<Account> _userManager;
    Services.IEmailSender _emailSender;
    Services.IAccountTokenGenerator _tokenGenerator;
     
    public TwoFactorController(UserManager<Account> userManager, Services.IAccountTokenGenerator generator, Services.IEmailSender emailSender)
    {
        _userManager = userManager;
        _tokenGenerator = generator;
        _emailSender = emailSender;
    }
    
    #region 2FA setting control
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Enable()
    {
        var claims = User.Claims.ToList();
        var email = claims.Single(x => x.Type == ClaimTypes.Email).Value;

        var user = await _userManager.FindByEmailAsync(email);

        await _userManager.SetTwoFactorEnabledAsync(user, true);

        return Ok();
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Disable()
    {
        var claims = User.Claims.ToList();
        var email = claims.Single(x => x.Type == ClaimTypes.Email).Value;

        var user = await _userManager.FindByEmailAsync(email);

        await _userManager.SetTwoFactorEnabledAsync(user, false);

        return Ok();
    }
    #endregion

    #region 2FA Process 
    [HttpPost]
    public async Task<IActionResult> Send()
    {
        var claims = User.Claims.ToList();
        var email = claims.Single(x => x.Type == ClaimTypes.Email).Value;

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound();

        var token = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
        var message = $"Your 2FA code is: {token}";
        await _emailSender.SendEmailAsync(user.Email, "2FA Code", message);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Verify([FromBody] string code)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, code);
        if (!isValid) return BadRequest("Invalid 2FA code");

        return Ok(new {Token = _tokenGenerator.GenerateJwtToken(user)});
    }
    #endregion
}