namespace Hms.AuthApi.Entities;

public class User : BaseEntity
{
    public string MobileNumber { get; set; } = default!;
    public string? LoginId { get; set; }
    public string? PasswordHash { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPasswordLoginEnabled { get; set; } = false;
    public bool IsOtpLoginEnabled { get; set; } = true;
    public bool IsFirstLoginCompleted { get; set; } = false;
    public string? PhotoUrl { get; set; }

    public StaffUser? StaffUser { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
