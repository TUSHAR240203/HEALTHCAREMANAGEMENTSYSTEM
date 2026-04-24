using Frontend.Models.Reception;
using System.Threading.Tasks;

namespace Frontend.Services
{
    public interface IReceptionApiService
    {
        Task<ReceptionPatientSearchResponseDto?> SearchPatientsAsync(ReceptionPatientSearchRequestDto request);
        Task<ReceptionPatientSummaryDto?> GetPatientSummaryAsync(int patientId);
        Task<T?> RegisterPatientAsync<T>(RegisterPatientByReceptionRequestDto request);
        Task<T?> VerifyPatientAsync<T>(int patientId, VerifyPatientRequestDto request);
        Task<T?> BookAppointmentAsync<T>(BookAppointmentRequestDto request);
        Task<T?> CheckInAsync<T>(CheckInRequestDto request);
        Task<DepartmentQueueResponseDto?> GetQueueAsync(int departmentId, DateOnly date);
        Task<QueueCurrentResponseDto?> GetCurrentQueueAsync(int departmentId, DateOnly date);
        Task<T?> CallNextAsync<T>(int departmentId, DateOnly date);
        Task<T?> StartTokenAsync<T>(int queueTokenId);
        Task<T?> CompleteTokenAsync<T>(int queueTokenId, CompleteQueueTokenRequestDto request);
        Task<T?> SkipTokenAsync<T>(int queueTokenId);
        Task<T?> RecallTokenAsync<T>(int queueTokenId);
        Task<T?> CancelTokenAsync<T>(int queueTokenId, CancelQueueTokenRequestDto request);
    }
}
