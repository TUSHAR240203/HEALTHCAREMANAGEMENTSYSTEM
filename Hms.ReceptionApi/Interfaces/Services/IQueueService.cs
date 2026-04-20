using Hms.ReceptionApi.DTOs.Reception;

namespace Hms.ReceptionApi.Interfaces.Services;

public interface IQueueService
{
    Task<DepartmentQueueResponseDto> GetDepartmentQueueAsync(int departmentId, DateOnly date);
    Task<QueueCurrentResponseDto?> GetCurrentAsync(int departmentId, DateOnly date);
    Task<QueueActionResponseDto?> CallNextAsync(int departmentId, DateOnly date);
    Task<QueueActionResponseDto?> StartAsync(int queueTokenId);
    Task<QueueActionResponseDto?> CompleteAsync(int queueTokenId, CompleteQueueTokenRequestDto request);
    Task<QueueActionResponseDto?> SkipAsync(int queueTokenId);
    Task<QueueActionResponseDto?> RecallAsync(int queueTokenId);
    Task<QueueActionResponseDto?> CancelAsync(int queueTokenId, CancelQueueTokenRequestDto request);
}