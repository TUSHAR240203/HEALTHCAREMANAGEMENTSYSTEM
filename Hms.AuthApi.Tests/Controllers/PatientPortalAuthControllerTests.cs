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
    public async Task SendPortalActivation_ShouldReturnOk()
    {
        var request = new SendPatientPortalActivationRequestDto
        {
            PatientId = 1
        };

        _authServiceMock
            .Setup(x => x.SendPortalActivationAsync(request))
            .Returns(Task.CompletedTask);

        var result = await _controller.SendPortalActivation(request);

        Assert.IsType<OkObjectResult>(result);
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

        var response = new AuthResponseDto();

        _authServiceMock
            .Setup(x => x.VerifyOtpAndActivateAsync(request))
            .ReturnsAsync(response);

        var result = await _controller.VerifyOtp(request);

        Assert.IsType<OkObjectResult>(result);
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

        var response = new AuthResponseDto();

        _authServiceMock
            .Setup(x => x.PatientLoginAsync(request))
            .ReturnsAsync(response);

        var result = await _controller.PatientLogin(request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task SendLoginOtp_ShouldReturnOk()
    {
        var request = new SendPatientPortalActivationRequestDto
        {
            PatientId = 1
        };

        _authServiceMock
            .Setup(x => x.SendLoginOtpAsync(1))
            .Returns(Task.CompletedTask);

        var result = await _controller.SendLoginOtp(request);

        Assert.IsType<OkObjectResult>(result);
    }
}