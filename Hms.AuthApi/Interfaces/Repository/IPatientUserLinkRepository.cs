using Hms.AuthApi.Entities;

namespace Hms.AuthApi.Interfaces.Repository;

public interface IPatientUserLinkRepository
{
    Task<PatientUserLink?> GetByPatientIdAsync(int patientId);
    Task<PatientUserLink?> GetByUserIdAsync(int userId);
    Task AddAsync(PatientUserLink link);
    Task SaveChangesAsync();
}