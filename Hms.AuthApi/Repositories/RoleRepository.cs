using Hms.AuthApi.Data;
using Hms.AuthApi.Entities;
using Hms.AuthApi.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace Hms.AuthApi.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AuthDbContext _context;

    public RoleRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByNameAsync(string roleName)
    {
        var normalized = roleName.Trim().ToUpper();

        return await _context.Roles
            .FirstOrDefaultAsync(x => x.NormalizedName == normalized && !x.IsDeleted);
    }
}