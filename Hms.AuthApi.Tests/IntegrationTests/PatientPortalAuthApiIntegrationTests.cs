using Hms.AuthApi.Data;
using Hms.AuthApi.DTOs.Auth;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Hms.AuthApi.Tests.IntegrationTests;

public class PatientPortalAuthApiIntegrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PatientPortalAuthApiIntegrationTests(
        WebApplicationFactory<Program> factory)
    {
        var appFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    x => x.ServiceType ==
                    typeof(DbContextOptions<AuthDbContext>)
                );

                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<AuthDbContext>(options =>
                {
                    options.UseInMemoryDatabase("PatientPortalTestDb");
                });
            });
        });

        _client = appFactory.CreateClient();
    }

    [Fact]
    public async Task SendPortalActivation_ShouldReturnOk()
    {
        var request = new SendPatientPortalActivationRequestDto
        {
            PatientId = 1
        };

        var response = await _client.PostAsJsonAsync(
            "/api/auth/patient/send-portal-activation",
            request
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task VerifyOtp_ShouldReturnOk()
    {
        var request = new VerifyOtpRequestDto
        {
            PatientId = 1,
            MobileNumber = "9999999999",
            OtpCode = "123456",
            Purpose = "Activation"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/auth/patient/verify-otp",
            request
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PatientLogin_ShouldReturnOk()
    {
        var request = new PatientLoginRequestDto
        {
            PatientId = 1,
            MobileNumber = "9999999999",
            OtpCode = "123456"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/auth/patient/login",
            request
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SendLoginOtp_ShouldReturnOk()
    {
        var request = new SendPatientPortalActivationRequestDto
        {
            PatientId = 1
        };

        var response = await _client.PostAsJsonAsync(
            "/api/auth/patient/send-login-otp",
            request
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}