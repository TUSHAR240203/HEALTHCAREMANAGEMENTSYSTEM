namespace Hms.ReceptionApi.DTOs.Doctors;

public class DoctorSummaryDto
{
    public int Id { get; set; }
    public string DoctorCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Specialization { get; set; } = default!;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = default!;
    public bool IsActive { get; set; }
    public bool SupportsTeleConsultation { get; set; }
}
