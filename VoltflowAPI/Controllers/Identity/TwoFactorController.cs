using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VoltflowAPI.Models.Endpoints;

namespace VoltflowAPI.Controllers.Identity;

/// <summary>
/// Managers Two Factor login
/// </summary>
[Route("api/Identity/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
    [HttpGet("status")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetStatus()
    {

        var claims = User.Claims.ToList();
        var email = claims.Single(x => x.Type == ClaimTypes.Email).Value;

        var user = await _userManager.FindByEmailAsync(email);

        return Ok(new { user.TwoFactorEnabled });
    }

    [HttpPost("enable")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Enable()
    {
        var claims = User.Claims.ToList();
        var email = claims.Single(x => x.Type == ClaimTypes.Email).Value;

        var user = await _userManager.FindByEmailAsync(email);

        await _userManager.SetTwoFactorEnabledAsync(user, true);

        return Ok();
    }

    [HttpPost("disable")]
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
    [HttpPost("send")]
    public async Task<IActionResult> Send()
    {
        var claims = User.Claims.ToList();

        var email = claims.Single(x => x.Type == "TwoFactorEmail").Value;

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound();

        var token = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
        var message = $"Your 2FA code is: {token}";
        await _emailSender.SendEmailAsync(user.Email, "2FA Code", message);

        return Ok();
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] TokenModel model)
    {
        var claims = User.Claims.ToList();
        var email = claims.Single(x => x.Type == "TwoFactorEmail").Value;

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return Unauthorized();

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, model.Token);
        if (!isValid)
            return BadRequest("Invalid 2FA code");

        //Generate token
        var roles = await _userManager.GetRolesAsync(user);
        bool isAdmin = false;

        if (roles.Contains("Admin"))
            isAdmin = true;

        var token = _tokenGenerator.GenerateJwtToken(user, isAdmin);

        return Ok(new { Token = token });
    }

    #endregion
}