using Hms.AuthApi.Data;
using Hms.AuthApi.Entities;
using Hms.AuthApi.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace Hms.AuthApi.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _context;

    public UserRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByMobileAsync(string mobileNumber)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x => x.MobileNumber == mobileNumber && !x.IsDeleted);
    }

    public async Task<User?> GetByMobileWithRolesAsync(string mobileNumber)
    {
        return await _context.Users
            .Include(x => x.StaffUser)
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.MobileNumber == mobileNumber && !x.IsDeleted);
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted);
    }

    public async Task<User?> GetByIdWithRolesAsync(int userId)
    {
        return await _context.Users
            .Include(x => x.StaffUser)
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted);
    }

    public async Task<User?> GetByLoginIdWithRolesAsync(string loginId)
    {
        return await _context.Users
            .Include(x => x.StaffUser)
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.LoginId == loginId && !x.IsDeleted);
    }

    public async Task<List<User>> GetAllWithRolesAsync()
    {
        return await _context.Users
            .Include(x => x.StaffUser)
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Id)
            .ToListAsync();
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}