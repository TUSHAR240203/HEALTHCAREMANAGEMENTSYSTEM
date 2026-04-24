using Hms.ReceptionApi.DTOs.Reception;
using Hms.ReceptionApi.Interfaces.Repository;
using Hms.ReceptionApi.Interfaces.Services;

namespace Hms.ReceptionApi.Services;

public class QueueService : IQueueService
{
    private readonly IQueueRepository _queueRepository;

    public QueueService(IQueueRepository queueRepository)
    {
        _queueRepository = queueRepository;
    }

    public async Task<DepartmentQueueResponseDto> GetDepartmentQueueAsync(int departmentId, DateOnly date)
    {
        var items = await _queueRepository.GetDepartmentQueueAsync(departmentId, date);
        return MapQueue(departmentId, $"Department {departmentId}", date, items);
    }

    public async Task<DepartmentQueueResponseDto> GetDoctorQueueAsync(int doctorId, DateOnly date)
    {
        var items = await _queueRepository.GetDoctorQueueAsync(doctorId, date);
        return MapQueue(doctorId, $"Doctor {doctorId}", date, items);
    }

    public async Task<QueueCurrentResponseDto?> GetCurrentAsync(int departmentId, DateOnly date)
    {
        var token = await _queueRepository.GetCurrentAsync(departmentId, date);
        return token == null ? null : MapCurrent(token);
    }

    public async Task<QueueCurrentResponseDto?> GetDoctorCurrentAsync(int doctorId, DateOnly date)
    {
        var token = await _queueRepository.GetDoctorCurrentAsync(doctorId, date);
        return token == null ? null : MapCurrent(token);
    }

    public async Task<QueueActionResponseDto?> CallNextAsync(int departmentId, DateOnly date)
    {
        var current = await _queueRepository.GetCurrentAsync(departmentId, date);
        if (current != null)
            throw new InvalidOperationException("A token is already active in this department.");

        var next = await _queueRepository.GetNextWaitingAsync(departmentId, date);
        if (next == null) return null;

        next.Status = "Called";
        next.CalledAtUtc = DateTime.UtcNow;
        next.UpdatedAtUtc = DateTime.UtcNow;

        await _queueRepository.UpdateAsync(next);
        await _queueRepository.SaveChangesAsync();

        return MapAction(next, "Next patient called successfully.");
    }

    public async Task<QueueActionResponseDto?> StartAsync(int queueTokenId)
    {
        var token = await _queueRepository.GetByIdAsync(queueTokenId);
        if (token == null) return null;

        if (token.Status != "Called")
            throw new InvalidOperationException("Only called tokens can be started.");

        token.Status = "InProgress";
        token.StartedAtUtc = DateTime.UtcNow;
        token.UpdatedAtUtc = DateTime.UtcNow;

        await _queueRepository.UpdateAsync(token);
        await _queueRepository.SaveChangesAsync();

        return MapAction(token, "Consultation started successfully.");
    }

    public async Task<QueueActionResponseDto?> CompleteAsync(int queueTokenId, CompleteQueueTokenRequestDto request)
    {
        var token = await _queueRepository.GetByIdAsync(queueTokenId);
        if (token == null) return null;

        if (token.Status != "InProgress" && token.Status != "Called")
            throw new InvalidOperationException("Only active tokens can be completed.");

        token.Status = "Completed";
        token.CompletedAtUtc = DateTime.UtcNow;
        token.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        token.UpdatedAtUtc = DateTime.UtcNow;

        await _queueRepository.UpdateAsync(token);
        await _queueRepository.SaveChangesAsync();

        return MapAction(token, "Token completed successfully.");
    }

    public async Task<QueueActionResponseDto?> SkipAsync(int queueTokenId)
    {
        var token = await _queueRepository.GetByIdAsync(queueTokenId);
        if (token == null) return null;

        if (token.Status != "Called")
            throw new InvalidOperationException("Only called tokens can be skipped.");

        token.Status = "Skipped";
        token.SkippedAtUtc = DateTime.UtcNow;
        token.UpdatedAtUtc = DateTime.UtcNow;

        await _queueRepository.UpdateAsync(token);
        await _queueRepository.SaveChangesAsync();

        return MapAction(token, "Token skipped successfully.");
    }

    public async Task<QueueActionResponseDto?> RecallAsync(int queueTokenId)
    {
        var token = await _queueRepository.GetByIdAsync(queueTokenId);
        if (token == null) return null;

        if (token.Status != "Skipped")
            throw new InvalidOperationException("Only skipped tokens can be recalled.");

        token.Status = "Called";
        token.CalledAtUtc = DateTime.UtcNow;
        token.UpdatedAtUtc = DateTime.UtcNow;

        await _queueRepository.UpdateAsync(token);
        await _queueRepository.SaveChangesAsync();

        return MapAction(token, "Token recalled successfully.");
    }

    public async Task<QueueActionResponseDto?> CancelAsync(int queueTokenId, CancelQueueTokenRequestDto request)
    {
        var token = await _queueRepository.GetByIdAsync(queueTokenId);
        if (token == null) return null;

        if (token.Status == "Completed")
            throw new InvalidOperationException("Completed token cannot be cancelled.");

        token.Status = "Cancelled";
        token.CancelledAtUtc = DateTime.UtcNow;
        token.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        token.UpdatedAtUtc = DateTime.UtcNow;

        await _queueRepository.UpdateAsync(token);
        await _queueRepository.SaveChangesAsync();

        return MapAction(token, "Token cancelled successfully.");
    }

    private static DepartmentQueueResponseDto MapQueue(int ownerId, string ownerName, DateOnly date, List<Entities.QueueToken> items)
    {
        return new DepartmentQueueResponseDto
        {
            DepartmentId = ownerId,
            DepartmentName = ownerName,
            Date = date,
            Queue = items.Select(x => new QueueItemDto
            {
                QueueTokenId = x.Id,
                TokenNumber = x.TokenNumber,
                PatientId = x.PatientId,
                UHID = x.UHID,
                PatientName = x.PatientName,
                AppointmentId = x.AppointmentId,
                DoctorId = x.DoctorId,
                Status = x.Status
            }).ToList()
        };
    }

    private static QueueCurrentResponseDto MapCurrent(Entities.QueueToken token)
    {
        return new QueueCurrentResponseDto
        {
            QueueTokenId = token.Id,
            TokenNumber = token.TokenNumber,
            PatientId = token.PatientId,
            UHID = token.UHID,
            PatientName = token.PatientName,
            AppointmentId = token.AppointmentId,
            DoctorId = token.DoctorId,
            Status = token.Status,
            CalledAtUtc = token.CalledAtUtc,
            StartedAtUtc = token.StartedAtUtc
        };
    }

    private static QueueActionResponseDto MapAction(Entities.QueueToken token, string message)
    {
        return new QueueActionResponseDto
        {
            QueueTokenId = token.Id,
            TokenNumber = token.TokenNumber,
            PatientId = token.PatientId,
            UHID = token.UHID,
            PatientName = token.PatientName,
            Status = token.Status,
            Message = message
        };
    }
}
