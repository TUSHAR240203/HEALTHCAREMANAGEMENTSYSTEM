namespace Hms.ReceptionApi.Entities;

public class PatientCheckIn : BaseEntity
{
    public int PatientId { get; set; }
    public string UHID { get; set; } = default!;

    public int AppointmentId { get; set; }
    public int DoctorId { get; set; }
    public int DepartmentId { get; set; }

    public DateTime CheckInTimeUtc { get; set; }
    public int TokenNumber { get; set; }

    public string Status { get; set; } = "CheckedIn";
}