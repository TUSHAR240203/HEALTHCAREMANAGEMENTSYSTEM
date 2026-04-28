using Hms.DoctorsApi.DTOs.Appointments;

namespace Hms.DoctorsApi.Interfaces.Clients;

public interface IAppointmentsApiClient
{
    Task<List<AppointmentResponseDto>> GetByDoctorIdAsync(int doctorId);
    Task<AppointmentResponseDto?> StartAppointmentAsync(int appointmentId);
    Task<AppointmentResponseDto?> CompleteAppointmentAsync(int appointmentId, CompleteAppointmentRequestDto request);
    Task<AppointmentResponseDto?> AddAppointmentNotesAsync(int appointmentId, UpdateAppointmentNotesRequestDto request);
}
