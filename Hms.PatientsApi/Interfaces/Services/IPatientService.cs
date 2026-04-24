<<<<<<< HEAD
﻿using Hms.PatientsApi.DTOs.Patients;
=======
using Hms.PatientsApi.DTOs.Patients;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85

namespace Hms.PatientsApi.Interfaces.Services;

public interface IPatientService
{
    Task<PatientResponseDto> CreateAsync(CreatePatientRequestDto request);
    Task<PatientResponseDto?> GetByIdAsync(int id);
    Task<PatientResponseDto?> GetByUhidAsync(string uhid);
    Task<PatientResponseDto?> UpdateAsync(int id, UpdatePatientRequestDto request);
    Task<PatientSearchResponseDto> SearchAsync(PatientSearchRequestDto request);
    Task<bool> SoftDeleteAsync(int id);
<<<<<<< HEAD
}
=======
    Task<MobileNumberChangeOtpResponseDto> SendMobileNumberChangeOtpAsync(int patientId, RequestMobileNumberChangeOtpDto request);
    Task<MobileNumberChangeOtpResponseDto> VerifyMobileNumberChangeOtpAsync(int patientId, VerifyMobileNumberChangeOtpDto request);
    Task<PatientResponseDto?> CompleteProfileAsync(int id, CompletePatientProfileRequestDto request);
}
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
