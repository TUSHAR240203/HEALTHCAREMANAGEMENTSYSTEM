namespace Hms.ReceptionApi.DTOs.Reception;

public class CheckInResponseDto
{
    public int CheckInId { get; set; }
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string UHID { get; set; } = default!;
    public int TokenNumber { get; set; }
    public int QueuePosition { get; set; }
    public string Status { get; set; } = default!;
    public string Message { get; set; } = default!;
}