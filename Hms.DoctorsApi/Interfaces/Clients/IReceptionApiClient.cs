using Hms.DoctorsApi.DTOs.Queue;

namespace Hms.DoctorsApi.Interfaces.Clients;

public interface IReceptionApiClient
{
    Task<DoctorQueueResponseDto?> GetDoctorQueueAsync(int doctorId, DateOnly date);
    Task<DoctorQueueCurrentResponseDto?> GetDoctorCurrentQueueAsync(int doctorId, DateOnly date);
    Task StartQueueTokenAsync(int queueTokenId);
    Task CompleteQueueTokenAsync(int queueTokenId, string? notes);
}
