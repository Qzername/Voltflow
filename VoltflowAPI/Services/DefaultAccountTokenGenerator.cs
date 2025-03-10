﻿using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VoltflowAPI.Models.Settings;

namespace VoltflowAPI.Services;

public class DefaultAccountTokenGenerator : IAccountTokenGenerator
{
    readonly string jwtKey;

    public DefaultAccountTokenGenerator(IOptions<JwtSettings> options)
    {
        jwtKey = options.Value.Key;
    }

    public string GenerateJwtToken(Account user, bool isAdmin)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        List<Claim> claims = [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        ];

        if (isAdmin)
            claims.Add(new(ClaimTypes.Role, "Admin"));

        var token = new JwtSecurityToken(
            claims: claims.ToArray(),
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateTwoFactorToken(Account user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: [new("TwoFactorEmail", user.Email)],
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
