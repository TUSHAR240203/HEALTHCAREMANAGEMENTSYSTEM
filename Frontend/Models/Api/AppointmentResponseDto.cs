namespace Frontend.Models.Api;

public class AppointmentResponseDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string UHID { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly SlotStartTime { get; set; }
    public TimeOnly SlotEndTime { get; set; }
    public string VisitType { get; set; } = string.Empty;
    public string? ReasonForVisit { get; set; }
    public bool IsTeleConsultation { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? StatusText { get; set; }
    public string DisplayStatus => !string.IsNullOrWhiteSpace(StatusText) ? StatusText : Status.ToString();
    public string StatusCssClass => DisplayStatus.Replace(" ", "-").ToLowerInvariant();
    public string? CancellationReason { get; set; }
    public string? CompletionNotes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}