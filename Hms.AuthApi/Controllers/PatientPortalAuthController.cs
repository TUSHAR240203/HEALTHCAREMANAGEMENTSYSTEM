<<<<<<< HEAD
=======
using Hms.AuthApi.DTOs;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
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

    [HttpPost("send-portal-activation")]
<<<<<<< HEAD
    public async Task<IActionResult> SendPortalActivation([FromBody] SendPatientPortalActivationRequestDto request)
    {
        await _authService.SendPortalActivationAsync(request);
        return Ok(new { message = "Portal activation OTP sent successfully." });
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto request)
    {
        var result = await _authService.VerifyOtpAndActivateAsync(request);
=======
    public async Task<IActionResult> SendPortalActivation(
        [FromBody] SendPatientPortalActivationRequestDto request)
    {
        await _authService.SendPortalActivationAsync(request);

        return Ok(new
        {
            message = "Portal activation OTP sent successfully."
        });
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(
        [FromBody] VerifyOtpRequestDto request)
    {
        var result = await _authService.VerifyOtpAndActivateAsync(request);

>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
        return Ok(result);
    }

    [HttpPost("login")]
<<<<<<< HEAD
    public async Task<IActionResult> PatientLogin([FromBody] PatientLoginRequestDto request)
    {
        var result = await _authService.PatientLoginAsync(request);
        return Ok(result);
    }
    [HttpPost("send-login-otp")]
    public async Task<IActionResult> SendLoginOtp([FromBody] SendPatientPortalActivationRequestDto request)
    {
        await _authService.SendLoginOtpAsync(request.PatientId);
        return Ok(new { message = "Login OTP sent successfully." });
=======
    public async Task<IActionResult> PatientLogin(
        [FromBody] LoginRequestDto request)
    {
        var result = await _authService.PatientLoginAsync(request);

        return Ok(result);
    }

    [HttpPost("send-login-otp")]
    public async Task<IActionResult> SendLoginOtp(
        [FromBody] SendPatientPortalActivationRequestDto request)
    {
        await _authService.SendLoginOtpAsync(
            request.PatientId,
            request.MobileNumber
        );

        return Ok(new
        {
            message = "Login OTP sent successfully."
        });
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    }
}