using Hms.AuthApi.Common;
using Hms.AuthApi.DTOs.Auth;
using Hms.AuthApi.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hms.AuthApi.Controllers;

[ApiController]
[Route("api/auth/patient")]
public class PatientPortalAuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public PatientPortalAuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("send-login-otp")]
    public async Task<IActionResult> SendLoginOtp([FromBody] SendPatientPortalActivationRequestDto request)
    {
        await _authService.SendLoginOtpAsync(request.PatientId, request.MobileNumber);

        return Ok(new ApiResponse<object>(
            true,
            "Login OTP sent successfully.",
            null
        ));
    }

    [HttpPost("login")]
    public async Task<IActionResult> PatientLogin([FromBody] LoginRequestDto request)
    {
        var result = await _authService.PatientLoginAsync(request);

        return Ok(new ApiResponse<object>(
            true,
            "Login successful.",
            result
        ));
    }
}