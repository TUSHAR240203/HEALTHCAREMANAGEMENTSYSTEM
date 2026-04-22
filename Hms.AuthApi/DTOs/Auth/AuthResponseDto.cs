namespace Hms.AuthApi.DTOs.Auth;

public class AuthResponseDto
{
    public int UserId { get; set; }
    public int PatientId { get; set; }
    public string UHID { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string MobileNumber { get; set; } = default!;
    public string[] Roles { get; set; } = [];
    public string AccessToken { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }
}
