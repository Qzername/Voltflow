using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VoltflowAPI.Contexts;

namespace VoltflowAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountsController : ControllerBase
{
    readonly UserManager<Account> _userManager;
    readonly IConfiguration _configuration;

    public AccountsController(UserManager<Account> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register()
    {
        var user = new Account { UserName="testUsername2", Phone="123456789", Name="testName", Surname="testSurname", Email = "tes2t@mail.com"};
        var result = await _userManager.CreateAsync(user, "password123@W");

        if (result.Succeeded)
            return Ok(new { message = "User registered successfully!" });

        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login()
    {
        var user = await _userManager.FindByNameAsync("testUsername2");  

        if (user != null && await _userManager.CheckPasswordAsync(user, "password123@W"))
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
}
