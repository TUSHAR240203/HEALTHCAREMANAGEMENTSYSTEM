using Hms.AppointmentsApi.DTOs.Appointments;

namespace Hms.AppointmentsApi.Interfaces.Services;

public interface IAppointmentService
{
    Task<AppointmentResponseDto> CreateAsync(CreateAppointmentRequestDto request);
    Task<AppointmentResponseDto?> GetByIdAsync(int id);
    Task<List<AppointmentResponseDto>> GetByPatientIdAsync(int patientId);
    Task<List<AppointmentResponseDto>> GetByDoctorIdAsync(int doctorId);
    Task<AppointmentSearchResponseDto> SearchAsync(AppointmentSearchRequestDto request);
    Task<AppointmentResponseDto?> RescheduleAsync(int id, RescheduleAppointmentRequestDto request);
    Task<AppointmentResponseDto?> CancelAsync(int id, CancelAppointmentRequestDto request);
    Task<AppointmentResponseDto?> StartAsync(int id);
    Task<AppointmentResponseDto?> CompleteAsync(int id, CompleteAppointmentRequestDto request);
    Task<AppointmentResponseDto?> AddNotesAsync(int id, UpdateAppointmentNotesRequestDto request);
}
