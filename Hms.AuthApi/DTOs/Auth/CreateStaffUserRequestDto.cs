namespace Hms.AuthApi.DTOs.Auth;

public class CreateStaffUserRequestDto
{
    public string LoginId { get; set; } = default!;
    public string? Password { get; set; }
    public string Role { get; set; } = default!;
    public string? Email { get; set; }
    public string? MobileNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public bool EnablePasswordLogin { get; set; } = false;
    public bool EnableOtpLogin { get; set; } = true;
}
