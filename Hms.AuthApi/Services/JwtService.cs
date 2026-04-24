using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hms.AuthApi.Entities;
using Hms.AuthApi.Interfaces.Services;
using Microsoft.IdentityModel.Tokens;

namespace Hms.AuthApi.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiresAtUtc) GenerateToken(User user, PatientUserLink? link)
    {
        var issuer = _configuration["Jwt:Issuer"]!;
        var audience = _configuration["Jwt:Audience"]!;
        var key = _configuration["Jwt:Key"]!;
        var expiryMinutes = int.Parse(_configuration["Jwt:AccessTokenExpiryMinutes"]!);

        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.MobilePhone, user.MobileNumber),
            new(ClaimTypes.Role, user.Role)
        };

        if (link != null)
        {
            claims.Add(new Claim("patientId", link.PatientId.ToString()));
            claims.Add(new Claim("uhid", link.UHID));
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}