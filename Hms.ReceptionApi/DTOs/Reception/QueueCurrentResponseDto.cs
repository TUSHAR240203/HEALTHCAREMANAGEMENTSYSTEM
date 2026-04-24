<<<<<<< HEAD
namespace Hms.ReceptionApi.DTOs.Reception;
=======
﻿namespace Hms.ReceptionApi.DTOs.Reception;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85

public class QueueCurrentResponseDto
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
    public DateTime? CalledAtUtc { get; set; }
    public DateTime? StartedAtUtc { get; set; }
}
=======
    public string Status { get; set; } = default!;
    public DateTime? CalledAtUtc { get; set; }
    public DateTime? StartedAtUtc { get; set; }
}
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
