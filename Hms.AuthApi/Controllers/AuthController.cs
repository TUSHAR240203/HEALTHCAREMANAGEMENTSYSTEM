using System.Security.Claims;
using Hms.AuthApi.Common;
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
            return Unauthorized(new ApiResponse<object>(
                false,
                "Unauthorized user.",
                null
            ));

        var result = await _authService.GetCurrentUserAsync(userId);
        if (result == null) return NotFound();

        return Ok(result);
    }
}