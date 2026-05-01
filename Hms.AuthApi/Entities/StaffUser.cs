namespace Hms.AuthApi.Entities;

public class StaffUser : BaseEntity
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;

    public User User { get; set; } = default!;
}
