namespace Hms.AuthApi.DTOs.Auth;

public class SendPatientPortalActivationRequestDto
{

    public int PatientId { get; set; }


    public string MobileNumber { get; set; } = string.Empty;
}