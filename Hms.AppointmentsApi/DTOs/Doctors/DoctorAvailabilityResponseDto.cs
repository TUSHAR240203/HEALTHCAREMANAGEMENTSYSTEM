namespace Hms.AppointmentsApi.DTOs.Doctors;

public class DoctorAvailabilityResponseDto
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = default!;
    public DateOnly Date { get; set; }
    public List<DoctorAvailabilitySlotDto> Slots { get; set; } = new();
}

public class DoctorAvailabilitySlotDto
{
    public TimeOnly SlotStartTime { get; set; }
    public TimeOnly SlotEndTime { get; set; }
    public bool IsAvailable { get; set; }
}
