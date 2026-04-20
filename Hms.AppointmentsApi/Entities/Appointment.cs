using Hms.AppointmentsApi.Enums;

namespace Hms.AppointmentsApi.Entities;

public class Appointment : BaseEntity
{
    public int PatientId { get; set; }
    public string UHID { get; set; } = default!;

    public int DoctorId { get; set; }
    public string? DoctorName { get; set; }

    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }

    public DateOnly AppointmentDate { get; set; }
    public TimeOnly SlotStartTime { get; set; }
    public TimeOnly SlotEndTime { get; set; }

    public string VisitType { get; set; } = default!;
    public string? ReasonForVisit { get; set; }
    public bool IsTeleConsultation { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;

    public string? CancellationReason { get; set; }
    public string? CompletionNotes { get; set; }
}