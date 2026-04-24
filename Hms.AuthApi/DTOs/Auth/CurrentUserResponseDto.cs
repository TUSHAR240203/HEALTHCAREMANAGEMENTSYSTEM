namespace Hms.AuthApi.DTOs.Auth;

public class CurrentUserResponseDto
{
    public int UserId { get; set; }
    public int? PatientId { get; set; }
    public string? UHID { get; set; }
<<<<<<< HEAD
    public string MobileNumber { get; set; } = default!;
    public string Role { get; set; } = default!;
}
=======
    public string? FullName { get; set; }
    public string MobileNumber { get; set; } = default!;
    public string[] Roles { get; set; } = [];
}
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
