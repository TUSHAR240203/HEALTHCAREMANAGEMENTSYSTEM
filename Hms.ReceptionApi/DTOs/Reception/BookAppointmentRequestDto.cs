namespace Hms.ReceptionApi.DTOs.Reception;

public class BookAppointmentRequestDto
{
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int DepartmentId { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly SlotStartTime { get; set; }
    public TimeOnly SlotEndTime { get; set; }
    public string VisitType { get; set; } = default!;
    public string? ReasonForVisit { get; set; }
    public bool IsTeleConsultation { get; set; }
}