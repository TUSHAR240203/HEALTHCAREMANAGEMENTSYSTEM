using Frontend.Models.Api;

namespace Frontend.Services
{
    public interface IAppointmentApiService
    {
        Task<AppointmentSearchResponseDto> SearchAsync(AppointmentSearchRequestDto request);
        Task<AppointmentResponseDto?> GetByIdAsync(int id);
        Task<List<AppointmentResponseDto>> GetByPatientIdAsync(int patientId);
        Task<List<AppointmentResponseDto>> GetByDoctorIdAsync(int doctorId);
        Task<AppointmentResponseDto> CreateAsync(CreateAppointmentRequestDto request);
        Task<AppointmentResponseDto?> RescheduleAsync(int id, RescheduleAppointmentRequestDto request);
        Task<AppointmentResponseDto?> CancelAsync(int id, CancelAppointmentRequestDto request);
        Task<AppointmentResponseDto?> CompleteAsync(int id, CompleteAppointmentRequestDto request);
    }
}