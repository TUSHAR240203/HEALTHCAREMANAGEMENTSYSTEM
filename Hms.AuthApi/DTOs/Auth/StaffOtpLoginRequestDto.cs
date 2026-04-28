namespace Hms.AuthApi.DTOs.Auth;

public class StaffOtpLoginRequestDto
{
    // Use LoginId for username/employee id OR mobile number. MobileNumber is kept for backward compatibility.
    public string? LoginId { get; set; }
    public string? MobileNumber { get; set; }
    public string OtpCode { get; set; } = default!;
}
