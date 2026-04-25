namespace Hms.DoctorsApi.DTOs.Doctors;

public class CreateDoctorRequestDto
{
    public string FullName { get; set; } = default!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public string? Qualification { get; set; }
    public string Specialization { get; set; } = default!;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = default!;
    public decimal ConsultationFee { get; set; }
    public int ExperienceYears { get; set; }
    public string? LicenseNumber { get; set; }
    public string? RoomNumber { get; set; }
    public bool SupportsTeleConsultation { get; set; }
    public string? PhotoUrl { get; set; }
}
