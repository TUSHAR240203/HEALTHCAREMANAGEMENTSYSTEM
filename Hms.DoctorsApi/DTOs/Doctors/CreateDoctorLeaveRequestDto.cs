namespace Hms.DoctorsApi.DTOs.Doctors;

public class CreateDoctorLeaveRequestDto
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Reason { get; set; }
}
