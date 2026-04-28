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

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await _authService.GetCurrentUserAsync(userId);

        return result == null ? NotFound() : Ok(result);
    }
    [Authorize]
    [HttpPut("me/photo-url")]
    public async Task<IActionResult> UpdateMyPhotoUrl([FromBody] UpdateProfilePhotoUrlRequestDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        if (request == null || string.IsNullOrWhiteSpace(request.PhotoUrl))
            return BadRequest(new { message = "Photo URL is required." });

        if (!request.PhotoUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase) &&
            !request.PhotoUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !request.PhotoUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Invalid photo URL." });

        var result = await _authService.UpdateProfilePhotoAsync(userId, request.PhotoUrl.Trim());

        return result == null ? NotFound(new { message = "Unable to update photo." }) : Ok(result);
    }


    [Authorize]
    [HttpPost("me/photo")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadMyPhoto(IFormFile photo)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        if (photo == null || photo.Length == 0)
            return BadRequest(new { message = "Please select a photo." });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return BadRequest(new
            {
                message = "Only JPG, JPEG, PNG, and WEBP images are allowed."
            });

        // Get current user to determine role
        var currentUser = await _authService.GetCurrentUserAsync(userId);

        if (currentUser == null)
            return NotFound();

        string folderName = "staff";

        if (currentUser.Roles != null &&
            currentUser.Roles.Contains(AppRoles.Receptionist))
        {
            folderName = "receptionists";
        }
        else if (currentUser.Roles != null &&
                 currentUser.Roles.Contains(AppRoles.Admin))
        {
            folderName = "admins";
        }
        else if (currentUser.Roles != null &&
                 currentUser.Roles.Contains(AppRoles.Doctor))
        {
            folderName = "doctors";
        }
        else if (currentUser.Roles != null &&
                 currentUser.Roles.Contains(AppRoles.Patient))
        {
            folderName = "patients";
        }
        // Create physical folder path
        var uploadsRoot = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            folderName
        );

        Directory.CreateDirectory(uploadsRoot);

        // Generate unique filename
        var fileName = $"{folderName}-{userId}-{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsRoot, fileName);

        // Save file physically
        await using (var stream = System.IO.File.Create(filePath))
        {
            await photo.CopyToAsync(stream);
        }

        // Save relative URL in DB
        var photoUrl = $"/uploads/{folderName}/{fileName}";

        var result = await _authService.UpdateProfilePhotoAsync(userId, photoUrl);

        if (result == null)
            return NotFound(new { message = "Unable to update photo." });

        return Ok(new
        {
            message = "Profile photo uploaded successfully.",
            photoUrl = photoUrl,
            data = result
        });
    }

    [HttpPost("staff/login")]
    [HttpPost("login")]
    public async Task<IActionResult> StaffLogin(
        [FromBody] StaffLoginRequestDto request)
    {
        return Ok(await _authService.StaffLoginAsync(request));
    }

    [HttpPost("staff/send-login-otp")]
    public async Task<IActionResult> SendStaffLoginOtp(
        [FromBody] StaffOtpRequestDto request)
    {
        await _authService.SendStaffLoginOtpAsync(request);

        return Ok(new
        {
            message = "Staff login OTP sent successfully."
        });
    }

    [HttpPost("staff/otp-login")]
    public async Task<IActionResult> StaffOtpLogin(
        [FromBody] StaffOtpLoginRequestDto request)
    {
        return Ok(await _authService.StaffOtpLoginAsync(request));
    }

    [Authorize]
    [HttpPut("auth-preference")]
    public async Task<IActionResult> UpdateAuthPreference(
        [FromBody] AuthPreferenceRequestDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        return Ok(await _authService.UpdateAuthPreferenceAsync(userId, request));
    }

    [HttpPost("bootstrap-admin")]
    public async Task<IActionResult> BootstrapAdmin(
        [FromBody] CreateStaffUserRequestDto request)
    {
        var users = await _authService.GetUsersAsync();

        if (users.Any())
            return Forbid();

        request.Role = AppRoles.Admin;

        return Ok(await _authService.CreateStaffUserAsync(request));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpGet("users")]
    public async Task<IActionResult> Users()
    {
        return Ok(await _authService.GetUsersAsync());
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateStaffUserRequestDto request)
    {
        return Ok(await _authService.CreateStaffUserAsync(request));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPut("users/{id:int}/status")]
    public async Task<IActionResult> SetStatus(
        int id,
        [FromBody] UpdateUserStatusRequestDto request)
    {
        var result = await _authService.SetUserActiveStatusAsync(
            id,
            request.IsActive
        );

        return result == null ? NotFound() : Ok(result);
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        return await _authService.SoftDeleteUserAsync(id)
            ? NoContent()
            : NotFound();
    }
}

public class UpdateProfilePhotoUrlRequestDto
{
    public string PhotoUrl { get; set; } = string.Empty;
}
