namespace Frontend.Models.Reception;

public class TodayAppointmentForCheckInDto
{
    public int AppointmentId { get; set; }

    public int PatientId { get; set; }

    public string? PatientName { get; set; }

    public string? UHID { get; set; }

    public int DoctorId { get; set; }

    public string? DoctorName { get; set; }

    public int DepartmentId { get; set; }

    public string? DepartmentName { get; set; }

    public DateOnly AppointmentDate { get; set; }

    public TimeOnly SlotStartTime { get; set; }

    public TimeOnly SlotEndTime { get; set; }

    public string? Status { get; set; }
}