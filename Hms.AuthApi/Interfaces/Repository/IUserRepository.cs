using Hms.AuthApi.Entities;

namespace Hms.AuthApi.Interfaces.Repository;

public interface IUserRepository
{
    Task<User?> GetByMobileAsync(string mobileNumber);
    Task<User?> GetByMobileWithRolesAsync(string mobileNumber);
    Task<User?> GetByIdAsync(int userId);
    Task<User?> GetByIdWithRolesAsync(int userId);
    Task<User?> GetByLoginIdWithRolesAsync(string loginId);
    Task<List<User>> GetAllWithRolesAsync();
    Task AddAsync(User user);
    Task SaveChangesAsync();
}
