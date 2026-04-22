using Hms.ReceptionApi.Entities;

namespace Hms.ReceptionApi.Interfaces.Repository;

public interface ICheckInRepository
{
    Task AddAsync(PatientCheckIn entity);
    Task<PatientCheckIn?> GetByIdAsync(int id);
    Task SaveChangesAsync();
}