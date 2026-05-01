namespace Hms.AppointmentsApi.Entities;

/// <summary>
/// Outbox record written when an appointment is completed.
/// A background service reads this and calls BillingApi.
/// This guarantees invoice creation even if BillingApi is temporarily down.
/// </summary>
public class AppointmentBillingOutbox
{
    public int Id { get; set; }

    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public string UHID { get; set; } = default!;

    public bool IsProcessed { get; set; } = false;

    /// <summary>Number of delivery attempts made by the background processor.</summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>Last error message from the background processor, if any.</summary>
    public string? LastError { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}
