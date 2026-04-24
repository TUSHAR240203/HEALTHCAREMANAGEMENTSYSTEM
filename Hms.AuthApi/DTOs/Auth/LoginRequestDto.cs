namespace Hms.AuthApi.DTOs.Auth;

<<<<<<< HEAD
public class PatientLoginRequestDto
=======
public class LoginRequestDto
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
{
    public int PatientId { get; set; }
    public string MobileNumber { get; set; } = default!;
    public string OtpCode { get; set; } = default!;
}