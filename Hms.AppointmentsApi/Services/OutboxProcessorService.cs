using Hms.AppointmentsApi.Interfaces.Clients;
using Hms.AppointmentsApi.Interfaces.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hms.AppointmentsApi.Services;

/// <summary>
/// Background service that polls the AppointmentBillingOutbox table every 10 seconds
/// and forwards unprocessed records to BillingApi.
/// Retries up to 5 times before abandoning a record.
/// </summary>
public class OutboxProcessorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessorService> _logger;

    // Polling interval — configurable if needed via appsettings
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(10);

    public OutboxProcessorService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxProcessorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessorService started. Polling every {Interval}s.",
            PollingInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingRecordsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // Swallow top-level exceptions so the worker never crashes
                _logger.LogError(ex, "Unexpected error in OutboxProcessorService polling loop.");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }

        _logger.LogInformation("OutboxProcessorService stopped.");
    }

    private async Task ProcessPendingRecordsAsync(CancellationToken stoppingToken)
    {
        // Create a new DI scope per batch — EF DbContext is scoped, not singleton
        await using var scope = _scopeFactory.CreateAsyncScope();

        var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var billingClient = scope.ServiceProvider.GetRequiredService<IBillingApiClient>();

        var pending = await outboxRepo.GetPendingAsync();

        if (pending.Count == 0)
            return;

        _logger.LogInformation("Outbox processor found {Count} pending record(s).", pending.Count);

        foreach (var record in pending)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            try
            {
                await billingClient.NotifyAppointmentCompletedAsync(
                    record.AppointmentId,
                    record.PatientId,
                    record.UHID,
                    record.DoctorId);

                await outboxRepo.MarkProcessedAsync(record.Id);
                await outboxRepo.SaveChangesAsync();

                _logger.LogInformation(
                    "Outbox record {Id} processed. AppointmentId={AppointmentId}",
                    record.Id, record.AppointmentId);
            }
            catch (Exception ex)
            {
                var error = ex.Message;

                await outboxRepo.RecordFailureAsync(record.Id, error);
                await outboxRepo.SaveChangesAsync();

                _logger.LogWarning(
                    "Outbox record {Id} failed (attempt {Attempt}/5). AppointmentId={AppointmentId}. Error: {Error}",
                    record.Id, record.RetryCount + 1, record.AppointmentId, error);
            }
        }
    }
}
