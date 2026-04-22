namespace Hms.DoctorsApi.DTOs.Doctors;

public class CreateDoctorLeaveRequestDto
{
    public DateOnly LeaveDate { get; set; }
    public string? Reason { get; set; }
}
