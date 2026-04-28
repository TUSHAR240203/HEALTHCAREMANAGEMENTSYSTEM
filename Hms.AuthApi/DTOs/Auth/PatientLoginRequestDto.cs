namespace Hms.AuthApi.DTOs.Auth;

public class LoginRequestDto
{
    public int PatientId { get; set; }
    public string MobileNumber { get; set; } = default!;
    public string OtpCode { get; set; } = default!;
}