namespace Hms.DoctorsApi.DTOs.Queue;

public class DoctorQueueResponseDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = default!;
    public DateOnly Date { get; set; }
    public List<DoctorQueueItemDto> Queue { get; set; } = new();
}

public class DoctorQueueItemDto
{
    public int QueueTokenId { get; set; }
    public int TokenNumber { get; set; }
    public int PatientId { get; set; }
    public string UHID { get; set; } = default!;
    public string PatientName { get; set; } = default!;
    public int AppointmentId { get; set; }
    public int DoctorId { get; set; }
    public string Status { get; set; } = default!;
}
