namespace Hms.ReceptionApi.DTOs.Doctors;

public class DoctorSearchRequestDto
{
    public string? Name { get; set; }
    public string? Specialization { get; set; }
    public int? DepartmentId { get; set; }
    public bool? IsActive { get; set; }
    public bool? SupportsTeleConsultation { get; set; }
}
