namespace Hms.ReceptionApi.DTOs.Reception;

public class QueueItemDto
{
    public int QueueTokenId { get; set; }
    public int TokenNumber { get; set; }
    public int PatientId { get; set; }
    public string UHID { get; set; } = default!;
    public string PatientName { get; set; } = default!;
<<<<<<< HEAD
    public int AppointmentId { get; set; }
    public int DoctorId { get; set; }
    public string Status { get; set; } = default!;
}
=======
    public string Status { get; set; } = default!;
}
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
