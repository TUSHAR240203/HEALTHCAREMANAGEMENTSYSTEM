using System.Security.Claims;
using Hms.AuthApi.Controllers;
using Hms.AuthApi.DTOs.Auth;
using Hms.AuthApi.Interfaces.Services;
using Hms.AuthApi.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Hms.AuthApi.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Me_ShouldReturnUnauthorized_WhenUserIdClaimIsMissing()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        var result = await _controller.Me();

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Me_ShouldReturnUnauthorized_WhenUserIdClaimIsInvalid()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "abc")
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
            }
        };

        var result = await _controller.Me();

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Me_ShouldReturnNotFound_WhenUserNotFound()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = TestClaimsHelper.GetUser(1)
            }
        };

        _authServiceMock
            .Setup(x => x.GetCurrentUserAsync(1))
            .ReturnsAsync((CurrentUserResponseDto?)null);

        var result = await _controller.Me();

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Me_ShouldReturnOk_WhenUserExists()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = TestClaimsHelper.GetUser(1)
            }
        };

        var userData = new CurrentUserResponseDto
        {
            UserId = 1,
            PatientId = null,
            UHID = null,
            FullName = "Test User",
            MobileNumber = "9999999999",
            PhotoUrl = null,
            Roles = new[] { "Admin" },
            IsProfileCompleted = true
        };

        _authServiceMock
            .Setup(x => x.GetCurrentUserAsync(1))
            .ReturnsAsync(userData);

        var result = await _controller.Me();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }
}