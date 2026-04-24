namespace Hms.AuthApi.DTOs.Auth;

public class VerifyOtpRequestDto
{
    public int PatientId { get; set; }
    public string MobileNumber { get; set; } = default!;
    public string OtpCode { get; set; } = default!;
    public string Purpose { get; set; } = default!;
}