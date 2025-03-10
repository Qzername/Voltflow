﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using VoltflowAPI.Models.Endpoints;
using VoltflowAPI.Services;

namespace VoltflowAPI.Controllers.Identity;

[Route("api/Identity/[controller]")]
[ApiController]
public class PasswordResetController : ControllerBase
{
    readonly UserManager<Account> _userManager;
    readonly IEmailSender _emailSender;

    public PasswordResetController(UserManager<Account> userManager, IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendToken([FromBody] SendTokenModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
            return Unauthorized();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var message = $"Your password reset token is: {token}";
        await _emailSender.SendEmailAsync(user.Email!, "Password reset", message);

        return Ok();
    }

    [HttpPost("reset")]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordResetModel password)
    {
        //meet password criteria
        if (!Regex.IsMatch(password.Password, AuthenticationController.PasswordRegex))
            return BadRequest(new { InvalidPassword = true });

        var user = await _userManager.FindByEmailAsync(password.Email);

        if (user == null)
            return Unauthorized();

        var result = await _userManager.ResetPasswordAsync(user, password.TokenModel.Token, password.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }

    public struct SendTokenModel
    {
        public string Email { get; set; }
    }

    public struct PasswordResetModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public TokenModel TokenModel { get; set; }
    }
}
