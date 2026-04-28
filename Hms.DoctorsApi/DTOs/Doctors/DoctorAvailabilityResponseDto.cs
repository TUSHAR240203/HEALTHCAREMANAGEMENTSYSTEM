namespace Hms.DoctorsApi.DTOs.Doctors;

public class DoctorAvailabilityResponseDto
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = default!;
    public DateOnly Date { get; set; }
    public List<DoctorAvailabilitySlotDto> Slots { get; set; } = new();
}
