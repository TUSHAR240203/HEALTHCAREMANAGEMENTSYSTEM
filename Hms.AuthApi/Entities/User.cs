namespace Hms.AuthApi.Entities;

<<<<<<< HEAD
using Hms.AuthApi.Common;


=======
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
public class User : BaseEntity
{
    public string MobileNumber { get; set; } = default!;
    public string? Email { get; set; }
<<<<<<< HEAD
    public string Role { get; set; } = AppRoles.Patient;
    public bool IsActive { get; set; } = true;
}
=======
    public bool IsActive { get; set; } = true;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
