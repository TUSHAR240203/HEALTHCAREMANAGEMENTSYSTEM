using System.Security.Claims;
using Hms.AuthApi.Common;
using Hms.AuthApi.DTOs.Auth;
using Hms.AuthApi.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hms.AuthApi.Controllers;

[ApiController]
[Route("api/auth")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = GetCurrentUserId();

        if (userId <= 0)
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        var result = await _authService.GetCurrentUserAsync(userId);

        return result == null
            ? NotFound(new { message = "User not found." })
            : Ok(result);
    }

    [HttpPut("me/photo-url")]
    public async Task<IActionResult> UpdateMyPhotoUrl(
        [FromBody] UpdateProfilePhotoUrlRequestDto request)
    {
        var userId = GetCurrentUserId();

        if (userId <= 0)
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        if (request == null || string.IsNullOrWhiteSpace(request.PhotoUrl))
            return BadRequest(new { message = "Photo URL is required." });

        var photoUrl = request.PhotoUrl.Trim();

        if (!photoUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase) &&
            !photoUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !photoUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Invalid photo URL." });
        }

        var result = await _authService.UpdateProfilePhotoAsync(userId, photoUrl);

        return result == null
            ? NotFound(new { message = "Unable to update photo." })
            : Ok(result);
    }

    [HttpPost("me/photo")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadMyPhoto(IFormFile photo)
    {
        var userId = GetCurrentUserId();

        if (userId <= 0)
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        if (photo == null || photo.Length == 0)
            return BadRequest(new { message = "Please select a photo." });

        if (photo.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "Photo size must be less than 5 MB." });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension))
        {
            return BadRequest(new
            {
                message = "Only JPG, JPEG, PNG, and WEBP images are allowed."
            });
        }

        var allowedContentTypes = new[]
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        if (string.IsNullOrWhiteSpace(photo.ContentType) ||
            !allowedContentTypes.Contains(photo.ContentType.ToLowerInvariant()))
        {
            return BadRequest(new
            {
                message = "Invalid image content type."
            });
        }

        var currentUser = await _authService.GetCurrentUserAsync(userId);

        if (currentUser == null)
            return NotFound(new { message = "User not found." });

        var folderName = GetUploadFolderName(currentUser.Roles);

        var uploadsRoot = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            folderName
        );

        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"{folderName}-{userId}-{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsRoot, fileName);

        await using (var stream = System.IO.File.Create(filePath))
        {
            await photo.CopyToAsync(stream);
        }

        var photoUrl = $"/uploads/{folderName}/{fileName}";

        var result = await _authService.UpdateProfilePhotoAsync(userId, photoUrl);

        if (result == null)
            return NotFound(new { message = "Unable to update photo." });

        return Ok(new
        {
            message = "Profile photo uploaded successfully.",
            photoUrl,
            data = result
        });
    }

    [AllowAnonymous]
    [HttpPost("staff/login")]
    [HttpPost("login")]
    public async Task<IActionResult> StaffLogin(
        [FromBody] StaffLoginRequestDto request)
    {
        if (request == null)
            return BadRequest(new { message = "Login request is required." });

        return Ok(await _authService.StaffLoginAsync(request));
    }

    [AllowAnonymous]
    [HttpPost("staff/send-login-otp")]
    public async Task<IActionResult> SendStaffLoginOtp(
        [FromBody] StaffOtpRequestDto request)
    {
        if (request == null)
            return BadRequest(new { message = "OTP request is required." });

        await _authService.SendStaffLoginOtpAsync(request);

        return Ok(new
        {
            message = "Staff login OTP sent successfully."
        });
    }

    [AllowAnonymous]
    [HttpPost("staff/otp-login")]
    public async Task<IActionResult> StaffOtpLogin(
        [FromBody] StaffOtpLoginRequestDto request)
    {
        if (request == null)
            return BadRequest(new { message = "OTP login request is required." });

        return Ok(await _authService.StaffOtpLoginAsync(request));
    }

    [HttpPut("auth-preference")]
    public async Task<IActionResult> UpdateAuthPreference(
        [FromBody] AuthPreferenceRequestDto request)
    {
        var userId = GetCurrentUserId();

        if (userId <= 0)
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        if (request == null)
            return BadRequest(new { message = "Auth preference request is required." });

        return Ok(await _authService.UpdateAuthPreferenceAsync(userId, request));
    }

    [AllowAnonymous]
    [HttpPost("bootstrap-admin")]
    public async Task<IActionResult> BootstrapAdmin(
        [FromBody] CreateStaffUserRequestDto request)
    {
        if (request == null)
            return BadRequest(new { message = "Admin user request is required." });

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
        if (request == null)
            return BadRequest(new { message = "User request is required." });

        return Ok(await _authService.CreateStaffUserAsync(request));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPut("users/{id:int}/status")]
    public async Task<IActionResult> SetStatus(
        int id,
        [FromBody] UpdateUserStatusRequestDto request)
    {
        if (id <= 0)
            return BadRequest(new { message = "Invalid user id." });

        if (request == null)
            return BadRequest(new { message = "Status request is required." });

        var result = await _authService.SetUserActiveStatusAsync(
            id,
            request.IsActive
        );

        return result == null
            ? NotFound(new { message = "User not found." })
            : Ok(result);
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        if (id <= 0)
            return BadRequest(new { message = "Invalid user id." });

        return await _authService.SoftDeleteUserAsync(id)
            ? NoContent()
            : NotFound(new { message = "User not found." });
    }

    private int GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            User.FindFirst("sub")?.Value ??
            User.FindFirst("userId")?.Value;

        return int.TryParse(userIdClaim, out var userId)
            ? userId
            : 0;
    }

    private static string GetUploadFolderName(IEnumerable<string>? roles)
    {
        if (roles == null)
            return "staff";

        var roleList = roles.ToList();

        if (roleList.Contains(AppRoles.Receptionist))
            return "receptionists";

        if (roleList.Contains(AppRoles.Admin))
            return "admins";

        if (roleList.Contains(AppRoles.Doctor))
            return "doctors";

        if (roleList.Contains(AppRoles.Patient))
            return "patients";

        return "staff";
    }
}