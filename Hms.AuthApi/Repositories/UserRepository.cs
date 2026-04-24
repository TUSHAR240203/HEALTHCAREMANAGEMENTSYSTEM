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
<<<<<<< HEAD
        return await _context.Users.FirstOrDefaultAsync(x => x.MobileNumber == mobileNumber && !x.IsDeleted);
=======
        return await _context.Users
            .FirstOrDefaultAsync(x => x.MobileNumber == mobileNumber && !x.IsDeleted);
    }

    public async Task<User?> GetByMobileWithRolesAsync(string mobileNumber)
    {
        return await _context.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.MobileNumber == mobileNumber && !x.IsDeleted);
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
<<<<<<< HEAD
        return await _context.Users.FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted);
=======
        return await _context.Users
            .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted);
    }

    public async Task<User?> GetByIdWithRolesAsync(int userId)
    {
        return await _context.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted);
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
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