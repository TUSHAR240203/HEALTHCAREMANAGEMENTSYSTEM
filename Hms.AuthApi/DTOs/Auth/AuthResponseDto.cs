namespace Hms.AuthApi.DTOs.Auth;

public class AuthResponseDto
{
    public int UserId { get; set; }
    public int PatientId { get; set; }
    public string UHID { get; set; } = default!;
<<<<<<< HEAD
    public string MobileNumber { get; set; } = default!;
    public string Role { get; set; } = default!;
    public string AccessToken { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }
}
=======
    public string FullName { get; set; } = default!;
    public string MobileNumber { get; set; } = default!;
    public string[] Roles { get; set; } = [];
    public string AccessToken { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsProfileCompleted { get; set; }
}
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
