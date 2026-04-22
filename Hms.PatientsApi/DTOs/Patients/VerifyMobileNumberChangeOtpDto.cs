namespace Hms.PatientsApi.DTOs.Patients;

public class VerifyMobileNumberChangeOtpDto
{
    public string MobileNumber { get; set; } = default!;
    public string OtpCode { get; set; } = default!;
}
