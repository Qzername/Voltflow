using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VoltflowAPI.Models.Endpoints;
using VoltflowAPI.Services;

namespace VoltflowAPI.Controllers.Identity;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    readonly UserManager<Account> _userManager;
    readonly IEmailSender _emailSender;
    readonly IAccountTokenGenerator _tokenGenerator;

    public AuthenticationController(UserManager<Account> userManager, IEmailSender emailSender, IAccountTokenGenerator generator)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _tokenGenerator = generator;
    }

    #region Register process
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user is not null)
            return BadRequest(new { AccountExist = true });

        user = new Account()
        {
            Email = model.Email,
            Name = model.Name,
            Surname = model.Surname,
            UserName = model.Email,
            PhoneNumber = model.Phone
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        //generate and send email confirmation token
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var message = $"Your e-mail confirmation token is: {token}";
        await _emailSender.SendEmailAsync(user.Email, "Email confirmation", message);

        return Ok();
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user is null)
            return BadRequest(new { AccountExist = false });

        var result = await _userManager.ConfirmEmailAsync(user, model.TokenModel.Token);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }
    #endregion

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
            return Unauthorized();

        if (await _userManager.IsEmailConfirmedAsync(user) == false)
            return Unauthorized();

        if (!await _userManager.CheckPasswordAsync(user, model.Password))
            return Unauthorized();

        //Two factor enabled -> move to TwoFactorController
        if (user.TwoFactorEnabled)
            return Ok(new { Requires2FA = true, TwoFactorToken = _tokenGenerator.GenerateTwoFactorToken(user) });

        var token = _tokenGenerator.GenerateJwtToken(user);
        return Ok(new { Token = token });
    }

    public struct RegisterModel
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
    }

    public struct ConfirmEmailModel
    {
        public string Email { get; set; }
        public TokenModel TokenModel { get; set; }
    }

    public struct LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
