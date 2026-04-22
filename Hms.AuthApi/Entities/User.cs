namespace Hms.AuthApi.Entities;

public class User : BaseEntity
{
    public string MobileNumber { get; set; } = default!;
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
