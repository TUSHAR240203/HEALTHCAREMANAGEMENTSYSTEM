using Frontend.Models.Reception;

namespace Frontend.Services
{
    public interface IReceptionApiService
    {
        // Patient search / registration
        Task<ReceptionPatientSearchResponseDto?> SearchPatientsAsync(ReceptionPatientSearchRequestDto request);

        Task<ReceptionPatientSummaryDto?> GetPatientSummaryAsync(int patientId);

        Task<List<TodayAppointmentForCheckInDto>> GetTodayScheduledAppointmentsForCheckInAsync(DateOnly date);

        Task<T?> RegisterPatientAsync<T>(RegisterPatientByReceptionRequestDto request);

        Task<T?> VerifyPatientAsync<T>(int patientId,
            VerifyPatientRequestDto request);

        // Appointment from reception
        Task<T?> BookAppointmentAsync<T>(BookAppointmentRequestDto request);

        // Patient check-in
        Task<T?> CheckInAsync<T>(CheckInRequestDto request);

        // Queue display
        Task<DepartmentQueueResponseDto?> GetQueueAsync(int departmentId,
            DateOnly date);

        Task<QueueCurrentResponseDto?> GetCurrentQueueAsync(int departmentId,
            DateOnly date);

        // Queue actions
        Task<T?> CallNextAsync<T>(int departmentId,DateOnly date);

        Task<T?> StartTokenAsync<T>(int queueTokenId);

        Task<T?> CompleteTokenAsync<T>(int queueTokenId,CompleteQueueTokenRequestDto request);

        Task<T?> SkipTokenAsync<T>(int queueTokenId);

        Task<T?> RecallTokenAsync<T>(int queueTokenId);

        Task<T?> CancelTokenAsync<T>(int queueTokenId,CancelQueueTokenRequestDto request);
    }
}