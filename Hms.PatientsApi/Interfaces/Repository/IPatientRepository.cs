<<<<<<< HEAD
﻿using Hms.PatientsApi.Entities;
using Hms.PatientsApi.DTOs.Patients;
=======
using Hms.PatientsApi.DTOs.Patients;
using Hms.PatientsApi.Entities;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85

namespace Hms.PatientsApi.Interfaces.Repository;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(int id);
    Task<Patient?> GetByUhidAsync(string uhid);
    Task<bool> ExistsByMobileAsync(string mobileNumber, int? excludePatientId = null);
<<<<<<< HEAD
    Task AddAsync(Patient patient);
    Task UpdateAsync(Patient patient);
    Task<PatientSearchResponseDto> SearchAsync(PatientSearchRequestDto request);
    Task SaveChangesAsync();
}
=======
    Task<Patient?> GetByMobileAsync(string mobileNumber, int? excludePatientId = null);
    Task AddAsync(Patient patient);
    Task UpdateAsync(Patient patient);
    Task<PatientSearchResponseDto> SearchAsync(PatientSearchRequestDto request);
    Task AddMobileNumberChangeRequestAsync(MobileNumberChangeRequest request);
    Task<MobileNumberChangeRequest?> GetLatestPendingMobileChangeRequestAsync(int patientId, string mobileNumber);
    Task SaveChangesAsync();
}
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
