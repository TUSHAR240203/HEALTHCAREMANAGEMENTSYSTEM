namespace Hms.DoctorsApi.Entities;

public class DoctorLeave : BaseEntity
{
    public int DoctorId { get; set; }
    public DateOnly LeaveDate { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime? ReviewedAtUtc { get; set; }
    public string? ReviewedBy { get; set; }

    public Doctor Doctor { get; set; } = default!;
}
