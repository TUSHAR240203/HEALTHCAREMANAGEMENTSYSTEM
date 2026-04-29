namespace Hms.DoctorsApi.DTOs.Doctors;

public class DoctorLeaveResponseDto
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public DateOnly LeaveDate { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ReviewedAtUtc { get; set; }
    public string? ReviewedBy { get; set; }
}
