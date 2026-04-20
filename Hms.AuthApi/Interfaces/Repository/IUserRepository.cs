using Hms.AuthApi.Entities;

namespace Hms.AuthApi.Interfaces.Repository;

public interface IUserRepository
{
    Task<User?> GetByMobileAsync(string mobileNumber);
    Task<User?> GetByIdAsync(int userId);
    Task AddAsync(User user);
    Task SaveChangesAsync();
}