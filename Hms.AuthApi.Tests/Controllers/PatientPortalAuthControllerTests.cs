using Hms.AuthApi.Controllers;
using Hms.AuthApi.DTOs.Auth;
using Hms.AuthApi.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Hms.AuthApi.Tests.Controllers;

public class PatientPortalAuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly PatientPortalAuthController _controller;

    public PatientPortalAuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new PatientPortalAuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task SendLoginOtp_ShouldReturnOk()
    {
        var request = new SendPatientPortalActivationRequestDto
        {
            PatientId = 1,
            MobileNumber = "9999999999"
        };

        _authServiceMock
            .Setup(x => x.SendLoginOtpAsync(request.PatientId, request.MobileNumber))
            .Returns(Task.CompletedTask);

        var result = await _controller.SendLoginOtp(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        _authServiceMock.Verify(
            x => x.SendLoginOtpAsync(request.PatientId, request.MobileNumber),
            Times.Once
        );
    }

    [Fact]
    public async Task PatientLogin_ShouldReturnOk()
    {
        var request = new LoginRequestDto
        {
            PatientId = 1,
            MobileNumber = "9999999999",
            OtpCode = "123456"
        };

        var authResponse = new AuthResponseDto
        {
            UserId = 1,
            PatientId = 1,
            UHID = "UHID001",
            FullName = "Test Patient",
            MobileNumber = "9999999999",
            Roles = new[] { "Patient" },
            AccessToken = "fake-token",
            ExpiresAtUtc = DateTime.UtcNow.AddHours(1),
            IsProfileCompleted = true,
            IsPasswordLoginEnabled = false,
            IsOtpLoginEnabled = true,
            IsFirstLoginCompleted = false
        };

        _authServiceMock
            .Setup(x => x.PatientLoginAsync(request))
            .ReturnsAsync(authResponse);

        var result = await _controller.PatientLogin(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        _authServiceMock.Verify(
            x => x.PatientLoginAsync(request),
            Times.Once
        );
    }
}