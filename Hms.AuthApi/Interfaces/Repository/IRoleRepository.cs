using Hms.AuthApi.Entities;

namespace Hms.AuthApi.Interfaces.Repository;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string roleName);
}