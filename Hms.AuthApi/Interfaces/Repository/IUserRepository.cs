using Hms.AuthApi.Entities;

namespace Hms.AuthApi.Interfaces.Repository;

public interface IUserRepository
{
    Task<User?> GetByMobileAsync(string mobileNumber);
<<<<<<< HEAD
    Task<User?> GetByIdAsync(int userId);
=======
    Task<User?> GetByMobileWithRolesAsync(string mobileNumber);
    Task<User?> GetByIdAsync(int userId);
    Task<User?> GetByIdWithRolesAsync(int userId);
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    Task AddAsync(User user);
    Task SaveChangesAsync();
}