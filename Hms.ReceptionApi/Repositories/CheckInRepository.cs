using Hms.ReceptionApi.Data;
using Hms.ReceptionApi.Entities;
using Hms.ReceptionApi.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace Hms.ReceptionApi.Repositories;

public class CheckInRepository : ICheckInRepository
{
    private readonly ReceptionDbContext _context;

    public CheckInRepository(ReceptionDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PatientCheckIn entity)
    {
        await _context.PatientCheckIns.AddAsync(entity);
    }

    public async Task<PatientCheckIn?> GetByIdAsync(int id)
    {
        return await _context.PatientCheckIns.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}