using System.IdentityModel.Tokens.Jwt;
using Hms.AuthApi.Entities;
using Hms.AuthApi.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Hms.AuthApi.Tests.Services;

public class JwtServiceTests
{
    [Fact]
    public void GenerateToken_ShouldReturnValidToken()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Issuer", "HmsAuthApi" },
                { "Jwt:Audience", "HmsUsers" },
                { "Jwt:Key", "this-is-a-very-secure-key-for-jwt-123456" },
                { "Jwt:AccessTokenExpiryMinutes", "60" }
            })
            .Build();

        var service = new JwtService(config);

        var user = new User
        {
            Id = 1,
            MobileNumber = "9999999999",
            Role = "Patient"
        };

        var link = new PatientUserLink
        {
            PatientId = 10,
            UHID = "UHID001",
            UserId = 1
        };

        var result = service.GenerateToken(user, link);

        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.True(result.ExpiresAtUtc > DateTime.UtcNow);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);

        Assert.Equal("HmsAuthApi", token.Issuer);
        Assert.Contains(token.Claims, x => x.Type == "patientId" && x.Value == "10");
        Assert.Contains(token.Claims, x => x.Type == "uhid" && x.Value == "UHID001");
    }
}