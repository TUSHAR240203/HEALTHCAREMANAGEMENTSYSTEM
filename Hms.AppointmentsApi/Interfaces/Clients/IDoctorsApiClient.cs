using Hms.AppointmentsApi.DTOs.Doctors;

namespace Hms.AppointmentsApi.Interfaces.Clients;

public interface IDoctorsApiClient
{
    Task<DoctorSummaryDto?> GetDoctorByIdAsync(int doctorId);
    Task<DoctorAvailabilityResponseDto?> GetAvailableSlotsAsync(int doctorId, DateOnly date, bool isTeleConsultation);
}
