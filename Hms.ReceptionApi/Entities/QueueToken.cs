namespace Hms.ReceptionApi.Entities;

public class QueueToken : BaseEntity
{
    public int DepartmentId { get; set; }
    public DateOnly QueueDate { get; set; }
    public int TokenNumber { get; set; }

    public int PatientId { get; set; }
    public string UHID { get; set; } = default!;
    public string PatientName { get; set; } = default!;

    public int AppointmentId { get; set; }
    public int DoctorId { get; set; }

    public string Status { get; set; } = "Waiting";

    public DateTime? CalledAtUtc { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime? SkippedAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }

    public string? Notes { get; set; }
}