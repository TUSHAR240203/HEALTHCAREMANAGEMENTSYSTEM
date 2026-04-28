namespace Hms.PatientsApi.DTOs.Patients;

public class MobileNumberChangeOtpResponseDto
{
    public string Message { get; set; } = default!;
    public bool VerificationRequired { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
}
