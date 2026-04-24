namespace Hms.DoctorsApi.DTOs.Doctors;

public class DoctorAvailabilitySlotDto
{
    public TimeOnly SlotStartTime { get; set; }
    public TimeOnly SlotEndTime { get; set; }
    public bool IsAvailable { get; set; }
}
