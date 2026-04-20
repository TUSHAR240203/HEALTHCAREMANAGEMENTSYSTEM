namespace Hms.AuthApi.Entities;

using Hms.AuthApi.Common;


public class User : BaseEntity
{
    public string MobileNumber { get; set; } = default!;
    public string? Email { get; set; }
    public string Role { get; set; } = AppRoles.Patient;
    public bool IsActive { get; set; } = true;
}