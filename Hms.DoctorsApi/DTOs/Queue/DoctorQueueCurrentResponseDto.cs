namespace Hms.DoctorsApi.DTOs.Queue;

public class DoctorQueueCurrentResponseDto
{
    public int QueueTokenId { get; set; }
    public int TokenNumber { get; set; }
    public int PatientId { get; set; }
    public string UHID { get; set; } = default!;
    public string PatientName { get; set; } = default!;
    public int AppointmentId { get; set; }
    public int DoctorId { get; set; }
    public string Status { get; set; } = default!;
    public DateTime? CalledAtUtc { get; set; }
    public DateTime? StartedAtUtc { get; set; }
}
