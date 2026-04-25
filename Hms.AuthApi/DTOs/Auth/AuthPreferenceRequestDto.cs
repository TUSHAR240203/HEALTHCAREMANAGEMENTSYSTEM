namespace Hms.AuthApi.DTOs.Auth;

public class AuthPreferenceRequestDto
{
    public bool EnablePasswordLogin { get; set; }
    public bool EnableOtpLogin { get; set; }
    public string? LoginId { get; set; }
    public string? Password { get; set; }
}
