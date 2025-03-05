using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using VoltflowAPI.Models.Endpoints;
using VoltflowAPI.Services;

namespace VoltflowAPI.Controllers.Identity;

[Route("api/Identity/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    readonly UserManager<Account> _userManager;
    readonly IEmailSender _emailSender;
    readonly IAccountTokenGenerator _tokenGenerator;

    /*
     * Password requirements
     * 
     * Minimum eight characters, 
     * Maximum 32 characters, 
     * at least one uppercase letter, 
     * one lowercase letter, 
     * one number
     * one special character
     */
    public const string PasswordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,32}$";

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
        //email must contain @ and .
        if (!model.Email.Contains("@") || !model.Email.Contains("."))
            return BadRequest(new { InvalidEmail = true });

        //meet password criteria
        if (!Regex.IsMatch(model.Password, PasswordRegex))
            return BadRequest(new { InvalidPassword = true });

        //meet data criteria
        if (model.Name.Length > 100 ||
            model.Surname.Length > 100 ||
            model.Phone.Length != 9)
            return BadRequest(new { InvalidData = true });

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

        //if first user, give him admin
        if(_userManager.Users.Count() == 1)
        {
            user = await _userManager.FindByEmailAsync(user.Email);
            await _userManager.AddToRoleAsync(user, "Admin");
        }

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

        if (await _userManager.CheckPasswordAsync(user, model.Password) == false)
            return Unauthorized();

        //Two factor enabled -> move to TwoFactorController
        if (user.TwoFactorEnabled)
            return Ok(new { Requires2FA = true, TwoFactorToken = _tokenGenerator.GenerateTwoFactorToken(user) });

        var roles = await _userManager.GetRolesAsync(user);
        bool isAdmin = false;

        if (roles.Contains("Admin"))
            isAdmin = true;

        var token = _tokenGenerator.GenerateJwtToken(user, isAdmin);

        return Ok(new
        {
            Token = token,
        });
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
