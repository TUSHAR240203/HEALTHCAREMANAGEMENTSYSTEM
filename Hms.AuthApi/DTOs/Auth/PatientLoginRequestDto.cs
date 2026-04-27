namespace Hms.AuthApi.DTOs.Auth;

public class PatientLoginRequestDto
{
    public int PatientId { get; set; }
    public string MobileNumber { get; set; } = default!;
    public string OtpCode { get; set; } = default!;
}