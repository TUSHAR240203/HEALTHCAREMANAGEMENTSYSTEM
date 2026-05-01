using Hms.AppointmentsApi.Data;
using Hms.AppointmentsApi.Entities;
using Hms.AppointmentsApi.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace Hms.AppointmentsApi.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly AppointmentsDbContext _context;

    public OutboxRepository(AppointmentsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AppointmentBillingOutbox record)
    {
        await _context.BillingOutbox.AddAsync(record);
    }

    public async Task<List<AppointmentBillingOutbox>> GetPendingAsync()
    {
        return await _context.BillingOutbox
            .Where(x => !x.IsProcessed && x.RetryCount < 5)  // max 5 attempts total
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkProcessedAsync(int id)
    {
        var record = await _context.BillingOutbox.FindAsync(id);
        if (record != null)
        {
            record.IsProcessed = true;
            record.ProcessedAt = DateTime.UtcNow;
        }
    }

    public async Task RecordFailureAsync(int id, string errorMessage)
    {
        var record = await _context.BillingOutbox.FindAsync(id);
        if (record != null)
        {
            record.RetryCount++;
            record.LastError = errorMessage.Length > 500
                ? errorMessage[..500]
                : errorMessage;
        }
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
