using Hms.AuthApi.Common;
using Hms.AuthApi.DTOs.Auth;
using Hms.AuthApi.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hms.AuthApi.Controllers;

[ApiController]
[Route("api/auth/patient")]
[AllowAnonymous]
public class PatientPortalAuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public PatientPortalAuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("send-login-otp")]
    public async Task<IActionResult> SendLoginOtp(
        [FromBody] SendPatientPortalActivationRequestDto request)
    {
        if (request == null)
        {
            return BadRequest(new ApiResponse<object>(
                false,
                "OTP request is required.",
                null
            ));
        }

        if (request.PatientId <= 0)
        {
            return BadRequest(new ApiResponse<object>(
                false,
                "Valid patient id is required.",
                null
            ));
        }

        if (string.IsNullOrWhiteSpace(request.MobileNumber))
        {
            return BadRequest(new ApiResponse<object>(
                false,
                "Mobile number is required.",
                null
            ));
        }

        await _authService.SendLoginOtpAsync(
            request.PatientId,
            request.MobileNumber.Trim()
        );

        return Ok(new ApiResponse<object>(
            true,
            "Login OTP sent successfully.",
            null
        ));
    }

    [HttpPost("login")]
    public async Task<IActionResult> PatientLogin(
        [FromBody] LoginRequestDto request)
    {
        if (request == null)
        {
            return BadRequest(new ApiResponse<object>(
                false,
                "Login request is required.",
                null
            ));
        }

        var result = await _authService.PatientLoginAsync(request);

        return Ok(new ApiResponse<object>(
            true,
            "Login successful.",
            result
        ));
    }
}