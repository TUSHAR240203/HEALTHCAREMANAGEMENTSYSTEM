namespace Hms.AuthApi.DTOs.Auth;

public class CurrentUserResponseDto
{
    public int UserId { get; set; }
    public int? PatientId { get; set; }
    public string? UHID { get; set; }
    public string? FullName { get; set; }
    public string MobileNumber { get; set; } = default!;
    public string[] Roles { get; set; } = [];
}
