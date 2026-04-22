namespace Hms.AuthApi.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = default!;
    public string NormalizedName { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
