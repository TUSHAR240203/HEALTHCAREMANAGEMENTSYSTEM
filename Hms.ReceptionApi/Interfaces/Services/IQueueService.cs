<<<<<<< HEAD
using Hms.ReceptionApi.DTOs.Reception;
=======
﻿using Hms.ReceptionApi.DTOs.Reception;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85

namespace Hms.ReceptionApi.Interfaces.Services;

public interface IQueueService
{
    Task<DepartmentQueueResponseDto> GetDepartmentQueueAsync(int departmentId, DateOnly date);
<<<<<<< HEAD
    Task<DepartmentQueueResponseDto> GetDoctorQueueAsync(int doctorId, DateOnly date);
    Task<QueueCurrentResponseDto?> GetCurrentAsync(int departmentId, DateOnly date);
    Task<QueueCurrentResponseDto?> GetDoctorCurrentAsync(int doctorId, DateOnly date);
=======
    Task<QueueCurrentResponseDto?> GetCurrentAsync(int departmentId, DateOnly date);
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    Task<QueueActionResponseDto?> CallNextAsync(int departmentId, DateOnly date);
    Task<QueueActionResponseDto?> StartAsync(int queueTokenId);
    Task<QueueActionResponseDto?> CompleteAsync(int queueTokenId, CompleteQueueTokenRequestDto request);
    Task<QueueActionResponseDto?> SkipAsync(int queueTokenId);
    Task<QueueActionResponseDto?> RecallAsync(int queueTokenId);
    Task<QueueActionResponseDto?> CancelAsync(int queueTokenId, CancelQueueTokenRequestDto request);
<<<<<<< HEAD
}
=======
}
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
