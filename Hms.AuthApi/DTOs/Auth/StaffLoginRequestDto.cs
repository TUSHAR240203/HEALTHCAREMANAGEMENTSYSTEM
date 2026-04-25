namespace Hms.AuthApi.DTOs.Auth;

public class StaffLoginRequestDto
{
    public string LoginId { get; set; } = default!;
    public string Password { get; set; } = default!;
}
