namespace Hms.AuthApi.DTOs.Auth;

public class UserAdminResponseDto
{
    public int UserId { get; set; }
    public string? LoginId { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public string[] Roles { get; set; } = [];
    public bool IsPasswordLoginEnabled { get; set; }
    public bool IsOtpLoginEnabled { get; set; }
    public bool IsFirstLoginCompleted { get; set; }
}
