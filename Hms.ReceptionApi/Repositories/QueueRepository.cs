using Hms.ReceptionApi.Data;
using Hms.ReceptionApi.Entities;
using Hms.ReceptionApi.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace Hms.ReceptionApi.Repositories;

public class QueueRepository : IQueueRepository
{
    private readonly ReceptionDbContext _context;

    public QueueRepository(ReceptionDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetNextTokenNumberAsync(int departmentId, DateOnly queueDate)
    {
        var lastToken = await _context.QueueTokens
            .Where(x => x.DepartmentId == departmentId && x.QueueDate == queueDate)
            .OrderByDescending(x => x.TokenNumber)
            .Select(x => (int?)x.TokenNumber)
            .FirstOrDefaultAsync();

        return (lastToken ?? 0) + 1;
    }

    public async Task AddAsync(QueueToken entity)
    {
        await _context.QueueTokens.AddAsync(entity);
    }

    public async Task<List<QueueToken>> GetDepartmentQueueAsync(int departmentId, DateOnly queueDate)
    {
        return await _context.QueueTokens
            .Where(x => x.DepartmentId == departmentId && x.QueueDate == queueDate && !x.IsDeleted)
            .OrderBy(x => x.TokenNumber)
            .ToListAsync();
    }

<<<<<<< HEAD
    public async Task<List<QueueToken>> GetDoctorQueueAsync(int doctorId, DateOnly queueDate)
    {
        return await _context.QueueTokens
            .Where(x => x.DoctorId == doctorId && x.QueueDate == queueDate && !x.IsDeleted)
            .OrderBy(x => x.TokenNumber)
            .ToListAsync();
    }

=======
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    public async Task<QueueToken?> GetByIdAsync(int queueTokenId)
    {
        return await _context.QueueTokens
            .FirstOrDefaultAsync(x => x.Id == queueTokenId && !x.IsDeleted);
    }

    public async Task<QueueToken?> GetCurrentAsync(int departmentId, DateOnly queueDate)
    {
        return await _context.QueueTokens
            .Where(x =>
                x.DepartmentId == departmentId &&
                x.QueueDate == queueDate &&
                !x.IsDeleted &&
                (x.Status == "Called" || x.Status == "InProgress"))
            .OrderBy(x => x.TokenNumber)
            .FirstOrDefaultAsync();
    }

<<<<<<< HEAD
    public async Task<QueueToken?> GetDoctorCurrentAsync(int doctorId, DateOnly queueDate)
    {
        return await _context.QueueTokens
            .Where(x =>
                x.DoctorId == doctorId &&
                x.QueueDate == queueDate &&
                !x.IsDeleted &&
                (x.Status == "Called" || x.Status == "InProgress"))
            .OrderBy(x => x.TokenNumber)
            .FirstOrDefaultAsync();
    }

=======
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    public async Task<QueueToken?> GetNextWaitingAsync(int departmentId, DateOnly queueDate)
    {
        return await _context.QueueTokens
            .Where(x =>
                x.DepartmentId == departmentId &&
                x.QueueDate == queueDate &&
                !x.IsDeleted &&
                x.Status == "Waiting")
            .OrderBy(x => x.TokenNumber)
            .FirstOrDefaultAsync();
    }

    public Task UpdateAsync(QueueToken entity)
    {
        _context.QueueTokens.Update(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
