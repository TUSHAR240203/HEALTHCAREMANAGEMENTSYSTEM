namespace Hms.AuthApi.DTOs.Auth;

public class UpdateUserStatusRequestDto
{
    public bool IsActive { get; set; }
}
public class UpdateProfilePhotoUrlRequestDto
{
    public string PhotoUrl { get; set; } = string.Empty;
}