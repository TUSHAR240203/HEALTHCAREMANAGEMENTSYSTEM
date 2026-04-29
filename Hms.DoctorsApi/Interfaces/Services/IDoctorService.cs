using Hms.DoctorsApi.DTOs.Appointments;
using Hms.DoctorsApi.DTOs.Doctors;
using Hms.DoctorsApi.DTOs.Queue;

namespace Hms.DoctorsApi.Interfaces.Services;

public interface IDoctorService
{
    Task<DoctorResponseDto> CreateAsync(CreateDoctorRequestDto request);
    Task<DoctorResponseDto?> GetByIdAsync(int id);
    Task<List<DoctorResponseDto>> SearchAsync(DoctorSearchRequestDto request);
    Task<DoctorResponseDto?> UpdateAsync(int id, UpdateDoctorRequestDto request);
    Task<bool> SoftDeleteAsync(int id);

    Task<List<DoctorScheduleResponseDto>> GetSchedulesAsync(int doctorId);
    Task<DoctorScheduleResponseDto> AddScheduleAsync(int doctorId, CreateDoctorScheduleRequestDto request);
    Task<bool> DeleteScheduleAsync(int doctorId, int scheduleId);

    Task<List<DoctorLeaveResponseDto>> GetLeavesAsync(int doctorId);
    Task<List<DoctorLeaveResponseDto>> GetLeavesAsync(string? status = null);
    Task<DoctorLeaveResponseDto> AddLeaveAsync(int doctorId, CreateDoctorLeaveRequestDto request);
    Task<DoctorLeaveResponseDto?> ApproveLeaveAsync(int leaveId, string? reviewedBy);
    Task<DoctorLeaveResponseDto?> RejectLeaveAsync(int leaveId, string? reviewedBy);
    Task<bool> DeleteLeaveAsync(int doctorId, int leaveId);

    Task<DoctorAvailabilityResponseDto> GetAvailableSlotsAsync(int doctorId, DateOnly date, bool? isTeleConsultation);

    Task<List<AppointmentResponseDto>> GetTodayAppointmentsAsync(int doctorId);
    Task<List<AppointmentResponseDto>> GetUpcomingAppointmentsAsync(int doctorId);
    Task<DoctorQueueCurrentResponseDto?> GetCurrentQueueAsync(int doctorId, DateOnly date);
    Task<AppointmentResponseDto?> StartAppointmentAsync(int doctorId, int appointmentId);
    Task<AppointmentResponseDto?> CompleteAppointmentAsync(int doctorId, int appointmentId, CompleteAppointmentRequestDto request);
    Task<AppointmentResponseDto?> AddAppointmentNotesAsync(int doctorId, int appointmentId, UpdateAppointmentNotesRequestDto request);
}
