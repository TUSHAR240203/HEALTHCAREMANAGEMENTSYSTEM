using Hms.AppointmentsApi.Entities;

namespace Hms.AppointmentsApi.Interfaces.Repository;

public interface IOutboxRepository
{
    /// <summary>Saves a new outbox record when appointment is completed.</summary>
    Task AddAsync(AppointmentBillingOutbox record);

    /// <summary>Returns all unprocessed records ordered by CreatedAt ascending.</summary>
    Task<List<AppointmentBillingOutbox>> GetPendingAsync();

    /// <summary>Marks record as successfully delivered.</summary>
    Task MarkProcessedAsync(int id);

    /// <summary>Records a failed delivery attempt without marking processed.</summary>
    Task RecordFailureAsync(int id, string errorMessage);

    Task SaveChangesAsync();
}
