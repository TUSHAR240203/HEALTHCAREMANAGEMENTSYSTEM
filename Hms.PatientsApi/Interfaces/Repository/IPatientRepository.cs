using Hms.PatientsApi.Entities;
using Hms.PatientsApi.DTOs.Patients;

namespace Hms.PatientsApi.Interfaces.Repository;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(int id);
    Task<Patient?> GetByUhidAsync(string uhid);
    Task<bool> ExistsByMobileAsync(string mobileNumber, int? excludePatientId = null);
    Task AddAsync(Patient patient);
    Task UpdateAsync(Patient patient);
    Task<PatientSearchResponseDto> SearchAsync(PatientSearchRequestDto request);
    Task SaveChangesAsync();
}