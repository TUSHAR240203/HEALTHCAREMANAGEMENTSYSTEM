using Hms.ReceptionApi.Entities;

namespace Hms.ReceptionApi.Interfaces.Repository;

public interface IQueueRepository
{
    Task<int> GetNextTokenNumberAsync(int departmentId, DateOnly queueDate);
    Task AddAsync(QueueToken entity);
    Task<List<QueueToken>> GetDepartmentQueueAsync(int departmentId, DateOnly queueDate);
    Task<List<QueueToken>> GetDoctorQueueAsync(int doctorId, DateOnly queueDate);
    Task<QueueToken?> GetByIdAsync(int queueTokenId);
    Task<QueueToken?> GetCurrentAsync(int departmentId, DateOnly queueDate);
    Task<QueueToken?> GetDoctorCurrentAsync(int doctorId, DateOnly queueDate);
    Task<QueueToken?> GetNextWaitingAsync(int departmentId, DateOnly queueDate);
    Task UpdateAsync(QueueToken entity);
    Task SaveChangesAsync();
}
