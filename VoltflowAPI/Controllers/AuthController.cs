using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace VoltflowAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
	readonly UserManager<Account> _userManager;
	readonly IConfiguration _configuration;

	public AuthController(UserManager<Account> userManager, IConfiguration configuration)
	{
		_userManager = userManager;
		_configuration = configuration;
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
			Surname = model.Surname
		};

		var result = await _userManager.CreateAsync(user, model.Password);

		if (result.Succeeded)
			return Ok(new { message = "User registered successfully!" });

		return BadRequest(result.Errors);
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginModel model)
	{
		var user = await _userManager.FindByEmailAsync(model.Email);

		if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
		{
			var token = GenerateJwtToken(user);
			return Ok(new { Token = token });
		}

		return Unauthorized();
	}

	string GenerateJwtToken(Account user)
	{
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
		var token = new JwtSecurityToken(
			claims:
			[
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Email, user.Email)
			],
			expires: DateTime.UtcNow.AddHours(1),
			signingCredentials: creds
		);
		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	public struct RegisterModel
	{
		public string Email { get; set; }
		public string Name { get; set; }
		public string Surname { get; set; }
		public string Password { get; set; }
	}

	public struct LoginModel
	{
		public string Email { get; set; }
		public string Password { get; set; }
	}
}
