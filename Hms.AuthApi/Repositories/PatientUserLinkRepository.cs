using Hms.AuthApi.Data;
using Hms.AuthApi.Entities;
using Hms.AuthApi.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace Hms.AuthApi.Repositories;

public class PatientUserLinkRepository : IPatientUserLinkRepository
{
    private readonly AuthDbContext _context;

    public PatientUserLinkRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<PatientUserLink?> GetByPatientIdAsync(int patientId)
    {
        return await _context.PatientUserLinks.FirstOrDefaultAsync(x => x.PatientId == patientId && !x.IsDeleted);
    }

    public async Task<PatientUserLink?> GetByUserIdAsync(int userId)
    {
        return await _context.PatientUserLinks.FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted);
    }

    public async Task AddAsync(PatientUserLink link)
    {
        await _context.PatientUserLinks.AddAsync(link);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}