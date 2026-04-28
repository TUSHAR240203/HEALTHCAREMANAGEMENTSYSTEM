using System.IdentityModel.Tokens.Jwt;
using Hms.AuthApi.Common;
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
            LoginId = "9999999999",
            Email = "patient@test.com",
            IsActive = true,
            IsDeleted = false,
            IsOtpLoginEnabled = true,
            IsPasswordLoginEnabled = false,
            IsFirstLoginCompleted = false
        };

        var link = new PatientUserLink
        {
            PatientId = 10,
            UHID = "UHID001",
            UserId = 1
        };

        var roles = new[] { AppRoles.Patient };

        var result = service.GenerateToken(user, link, roles);

        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.True(result.ExpiresAtUtc > DateTime.UtcNow);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);

        Assert.Equal("HmsAuthApi", token.Issuer);
        Assert.Contains(token.Claims, x => x.Type == "patientId" && x.Value == "10");
        Assert.Contains(token.Claims, x => x.Type == "uhid" && x.Value == "UHID001");
        Assert.Contains(token.Claims, x => x.Type == "role" && x.Value == AppRoles.Patient);
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidToken_ForStaffUserWithoutPatientLink()
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
            Id = 2,
            MobileNumber = "8888888888",
            LoginId = "admin",
            Email = "admin@test.com",
            IsActive = true,
            IsDeleted = false,
            IsOtpLoginEnabled = true,
            IsPasswordLoginEnabled = true,
            IsFirstLoginCompleted = true
        };

        var roles = new[] { AppRoles.Admin };

        var result = service.GenerateToken(user, null, roles);

        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.True(result.ExpiresAtUtc > DateTime.UtcNow);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);

        Assert.Equal("HmsAuthApi", token.Issuer);
        Assert.Contains(token.Claims, x => x.Type == "role" && x.Value == AppRoles.Admin);
        Assert.DoesNotContain(token.Claims, x => x.Type == "patientId");
        Assert.DoesNotContain(token.Claims, x => x.Type == "uhid");
    }
}