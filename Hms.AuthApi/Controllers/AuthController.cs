using System.Security.Claims;
using Hms.AuthApi.Common;
using Hms.AuthApi.DTOs.Auth;
using Hms.AuthApi.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hms.AuthApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();
        var result = await _authService.GetCurrentUserAsync(userId);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("staff/login")]
    [HttpPost("login")]
    public async Task<IActionResult> StaffLogin([FromBody] StaffLoginRequestDto request)
        => Ok(await _authService.StaffLoginAsync(request));

    [HttpPost("staff/send-login-otp")]
    public async Task<IActionResult> SendStaffLoginOtp([FromBody] StaffOtpRequestDto request)
    {
        await _authService.SendStaffLoginOtpAsync(request);
        return Ok(new { message = "Staff login OTP sent successfully." });
    }

    [HttpPost("staff/otp-login")]
    public async Task<IActionResult> StaffOtpLogin([FromBody] StaffOtpLoginRequestDto request)
        => Ok(await _authService.StaffOtpLoginAsync(request));

    [Authorize]
    [HttpPut("auth-preference")]
    public async Task<IActionResult> UpdateAuthPreference([FromBody] AuthPreferenceRequestDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();
        return Ok(await _authService.UpdateAuthPreferenceAsync(userId, request));
    }

    [HttpPost("bootstrap-admin")]
    public async Task<IActionResult> BootstrapAdmin([FromBody] CreateStaffUserRequestDto request)
    {
        var users = await _authService.GetUsersAsync();
        if (users.Any()) return Forbid();
        request.Role = AppRoles.Admin;
        return Ok(await _authService.CreateStaffUserAsync(request));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpGet("users")]
    public async Task<IActionResult> Users() => Ok(await _authService.GetUsersAsync());

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateStaffUserRequestDto request)
        => Ok(await _authService.CreateStaffUserAsync(request));

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPut("users/{id:int}/status")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] UpdateUserStatusRequestDto request)
    {
        var result = await _authService.SetUserActiveStatusAsync(id, request.IsActive);
        return result == null ? NotFound() : Ok(result);
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
        => await _authService.SoftDeleteUserAsync(id) ? NoContent() : NotFound();
}
