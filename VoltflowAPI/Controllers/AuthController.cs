using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VoltflowAPI.Services;

namespace VoltflowAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    readonly UserManager<Account> _userManager;
    readonly IAccountTokenGenerator _tokenGenerator;

    public AuthController(UserManager<Account> userManager, IAccountTokenGenerator generator)
    {
        _userManager = userManager;
        _tokenGenerator = generator;
    }

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
            Phone = model.Phone
        };

		var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
            return Ok();

		return BadRequest(result.Errors);
	}

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        Console.WriteLine(model.Email);

        var user = await _userManager.FindByEmailAsync(model.Email);

        Console.WriteLine(user is null);

        if (user == null || !(await _userManager.CheckPasswordAsync(user, model.Password)))
            return Unauthorized();

        if (user.TwoFactorEnabled)
            return Ok(new { Requires2FA = true });

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

	public struct LoginModel
	{
		public string Email { get; set; }
		public string Password { get; set; }
	}
}
