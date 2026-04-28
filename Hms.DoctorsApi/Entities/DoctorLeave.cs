namespace Hms.DoctorsApi.Entities;

public class DoctorLeave : BaseEntity
{
    public int DoctorId { get; set; }
    public DateOnly LeaveDate { get; set; }
    public string? Reason { get; set; }

    public Doctor Doctor { get; set; } = default!;
}
