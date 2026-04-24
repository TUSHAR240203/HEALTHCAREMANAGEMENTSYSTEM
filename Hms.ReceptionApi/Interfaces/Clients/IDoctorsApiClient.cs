using Hms.ReceptionApi.DTOs.Doctors;

namespace Hms.ReceptionApi.Interfaces.Clients;

public interface IDoctorsApiClient
{
    Task<List<DoctorSummaryDto>> SearchDoctorsAsync(DoctorSearchRequestDto request);
    Task<DoctorSummaryDto?> GetDoctorByIdAsync(int doctorId);
    Task<DoctorAvailabilityResponseDto?> GetAvailableSlotsAsync(int doctorId, DateOnly date, bool isTeleConsultation);
}
