using Hms.AppointmentsApi.DTOs.Doctors;

namespace Hms.AppointmentsApi.Interfaces.Clients;

public interface IDoctorsApiClient
{
    Task<DoctorAvailabilityResponseDto?> GetAvailabilityAsync(
        int doctorId,
        DateOnly date,
        bool? isTeleConsultation);
}